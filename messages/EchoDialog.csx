#r "Newtonsoft.Json"
#r "System.Net.Http"
#load "DataFormat.csx"
#load "ParkopediaHelper.csx"

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    protected int count = 1;
    protected string location = "";
    protected DialogState currentState = DialogState.Start;

    protected enum DialogState
    {
        Start,
        WaitingForLocation,
        WaitingForPriority
    };

    //public EchoDialog() : base(new LuisService(new LuisModelAttribute(Utils.GetAppSetting("LuisAppId"), Utils.GetAppSetting("LuisAPIKey"))))
    //{

    //}

    //[LuisIntent("None")]
    //public async Task NoneIntent(IDialogContext context, LuisResult result)
    //{
    //    await context.PostAsync($"I'm not sure I can help you with that. I only knowabout parking."); //
    //    context.Wait(MessageReceived);
    //}

    //[LuisIntent("FindParking")]
    //public async Task MyIntent(IDialogContext context, LuisResult result)
    //{
    //    await context.PostAsync($"I'd like to be able to help with that parking need. Really. My creator hasn't given me that ability yet though. Kinda makes you doubt their existence. (You said: {result.Query})"); //
    //    context.Wait(MessageReceived);
    //}

    public Task StartAsync(IDialogContext context)
    {
        //var task = base.StartAsync(context);

        try
        {
            context.Wait(MessageReceivedAsync);
        }
        catch (OperationCanceledException error)
        {
            return Task.FromCanceled(error.CancellationToken);
        }
        catch (Exception error)
        {
            return Task.FromException(error);
        }

        return Task.CompletedTask;
    }

    static async Task<String> CallLuisParkingModelAsync(string message)
    {
        var httpClient = new HttpClient();
        var baseUrl = "https://api.projectoxford.ai/luis/v2.0/apps/fab0c79f-240e-41dc-bd80-b9df7d39b317?subscription-key=b219e1818788401bbbfbdfd38874e5dd&q=";
        var queryUrl = baseUrl + WebUtility.UrlEncode(message);
        var intent = string.Empty;

        var response = await httpClient.GetAsync(queryUrl);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var bodyData = JsonConvert.DeserializeObject<LuisResponse>(responseBody);
            intent = bodyData.topScoringIntent.intent;
        }
        return (intent);
    }

    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        var wait = true;

        var intent = await CallLuisParkingModelAsync(message.Text);

        if (intent == "Reset")
        {
            await context.PostAsync($"LUIS determined that you wanted to reset");
        }

        if ((currentState == DialogState.Start) || (intent == "Reset"))
        {
            await AfterResetAsync(context);
        }
        else if (currentState == DialogState.WaitingForLocation)
        {
            location = message.Text;
            currentState = DialogState.WaitingForPriority;

            PromptDialog.Choice(context, AfterSelectPriorityAsync, new List<string>(){
            "Price",
            "Availability",
            "Distance",
            "Rating",
            "(Start Over)"
            }, "What's most important to you?");

            wait = false;
        }

        if (wait)
        {
            context.Wait(MessageReceivedAsync);
        }
    }

    public async Task AfterResetAsync(IDialogContext context)
    {
        await context.PostAsync("Tell me where you want to park.");
        this.currentState = DialogState.WaitingForLocation;
    }

    public async Task AfterSelectPriorityAsync(IDialogContext context, IAwaitable<string> priority)
    {
        string reply = string.Empty;
        var choice = await priority;

        if (choice == "(Start Over)")
        {
            await AfterResetAsync(context);
            context.Wait(MessageReceivedAsync);
        }
        else
        {
            var sortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), choice);

            var helper = new ParkopediaApiHelper();
            var response = await helper.SearchForParkingAsync<ServiceResponse>(this.location,
                sortOrder);

            if (response.IsNoParking)
            {
                reply = $"I'm sorry, I can't find any spaces around {this.location} right now, check back later";
            }
            else if (response.IsValid)
            {
                var topThree = response.result.spaces.Take(3);
                var count = topThree.Count();

                reply = $"I found at least {count} car parks for you.";

                foreach (var carpark in topThree)
                {
                    reply += $"\n\n{carpark}";
                }
            }
            else
            {
                reply = $"I'm sorry, I couldn't find a car park for {this.location} prioritized by {sortOrder}. My friend says {response.error} ({response.errorcode}).";
            }

            await context.PostAsync(reply);
            await AfterResetAsync(context);
            context.Wait(MessageReceivedAsync);
        }
    }

}

public class LuisResponse
{
    public string query { get; set; }
    public Intent topScoringIntent { get; set; }
    public Intent[] intents { get; set; }
    public object[] entities { get; set; }
}

public class Intent
{
    public string intent { get; set; }
    public float score { get; set; }
}