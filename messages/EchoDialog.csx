#r "Newtonsoft.Json"
#load "DataFormat.csx"
#load "ParkopediaHelper.csx"

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : LuisDialog<object>
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

    public EchoDialog() : base(new LuisService(new LuisModelAttribute(Utils.GetAppSetting("LuisAppId"), Utils.GetAppSetting("LuisAPIKey"))))
    {

    }

    [LuisIntent("None")]
    public async Task NoneIntent(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"I'm not sure I can help you with that. I only knowabout parking."); //
        context.Wait(MessageReceived);
    }

    public Task StartAsync(IDialogContext context)
    {
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

    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        var wait = true;

        if ((currentState == DialogState.Start) || (message.Text == "reset") || (message.Text == "help"))
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