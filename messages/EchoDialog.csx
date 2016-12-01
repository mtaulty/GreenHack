#r "Newtonsoft.Json"
#load "DataFormat.csx"
#load "ParkopediaHelper.csx"

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    protected int count = 1;
    protected bool helpPrompt = false;
    protected string location = "";
    protected DialogState currentState = DialogState.Start;

    protected enum DialogState
    {
        Start,
        WaitingForPriority
    };

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

        if (!helpPrompt || (message.Text == "help"))
        {
            await context.PostAsync("Tell me where you want to park.");
            currentState = DialogState.Start;
            helpPrompt = true;
        }
        else if (currentState == DialogState.Start)
        {
            location = message.Text;
            currentState = DialogState.WaitingForPriority;

            PromptDialog.Choice(context, AfterSelectPriorityAsync, new List<string>(){
            "Price",
            "Availability",
            "Distance",
            "Rating"
            }, "What's most important to you?");

            wait = false;
        }

        if(wait)
        {
            context.Wait(MessageReceivedAsync);
        }
    }

    public async Task AfterSelectPriorityAsync(IDialogContext context, IAwaitable<string> priority)
    {
        string reply = string.Empty;

        var choice = await priority;
        var sortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), choice);

        var helper = new ParkopediaApiHelper();
        var response = await helper.SearchForParkingAsync<ServiceResponse>(this.location,
            sortOrder);

        if (response.IsValid)
        {
            var topThree = response.result.spaces.Take(3);
            var count = topThree.Count();

            if (count == 0)
            {
                reply = "I'm sorry, I didn't find any car parks at that location";
            }
            else
            {
                reply = $"I found at least {count} car parks for you";
            }
        }
        else
        {
            reply = $"I'm sorry, I couldn't find a car park for {this.location} prioritized by {sortOrder}. My friend says {response.error} ({response.errorcode}).";
        }

        await context.PostAsync(reply);

        currentState = DialogState.Start;
        context.Wait(MessageReceivedAsync);
    }

    public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            this.count = 1;
            await context.PostAsync("Reset count.");
        }
        else
        {
            await context.PostAsync("Did not reset count.");
        }
        context.Wait(MessageReceivedAsync);
    }
}