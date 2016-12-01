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
        var reply = "I'm a car park finder. Please tell me where you are looking to park.";

        if (!helpPrompt)
        {
            context.PostAsync($"Where do you want to park? Type 'help' to see what else I can do for you.");
            helpPrompt = true;
        }

        var helper = new ParkopediaApiHelper();

        var response = await helper.SearchForParkingAsync<ServiceResponse>(message.Text);

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
            //foreach (var item in response.result.spaces)
            //{
            //    Console.WriteLine(
            //      $"{item.city}, {string.Join(",", item.addresses)}, {item.lat}, {item.lng}, {item.phone}");
            //}
        }
        else
        {
            reply = "I'm sorry, I couldn't talk to my car park friend to find out. Call back later";
        }
        await context.PostAsync(reply);

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