#r "Newtonsoft.Json"
#load "DataFormat.csx"

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
        if (message.Text == "reset")
        {
            PromptDialog.Confirm(
                context,
                AfterResetAsync,
                "Are you sure you want to reset the count?",
                "Didn't get that!",
                promptStyle: PromptStyle.Auto);
        }
        else if (message.Text == "help")
        {
            await context.PostAsync($"I'm a car park finder. Please tell me where you are looking to park.");
            context.Wait(MessageReceivedAsync);
        }
        else
        {
            if(!helpPrompt)
            {
                context.PostAsync($"Where do you want to park? Type 'help' to see what else I can do for you.");
                helpPrompt = true;
            }

            var newclass = new Class1();

            var utility = new BotUtilities();
            await context.PostAsync(utility.FormatReply(count++, message.Text));
//            await context.PostAsync($"{this.count++}: You said {message.Text}");
            context.Wait(MessageReceivedAsync);
        }
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

public class BotUtilities
{
    public BotUtilities()
    {
    }

    public string FormatReply(int count, string message)
    {
        return ($"{count}: I think you said {message}");
    }
}