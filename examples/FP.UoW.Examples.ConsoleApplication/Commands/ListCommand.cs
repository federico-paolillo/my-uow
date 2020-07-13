using CliFx;
using CliFx.Attributes;
using FP.UoW.Examples.ConsoleApplication.Services;
using System;
using System.Threading.Tasks;

namespace FP.UoW.Examples.ConsoleApplication.Commands
{
    [Command("list")]
    public sealed class ListCommand : ICommand
    {
        private readonly ThingsService thingsService = null;

        public ListCommand(ThingsService thingsService)
        {
            this.thingsService = thingsService ?? throw new ArgumentNullException(nameof(thingsService));
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            //Irrelevant, third party code
            var cancellationToken = console.GetCancellationToken();

            //We call the ThingsService here to get the Things from the database
            var things = await thingsService.GetThingsAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            //Irrelevant, third party code to print the results to the Console
            console.Output.WriteLine("The following Things are stored in the database.");

            if (things.Count == 0)
            {
                console.Output.WriteLine("Ah, there are none.");
            }
            else
            {
                foreach (var thing in things)
                {
                    console.Output.WriteLine("---");

                    console.Output.WriteLine($">> Thing.Column_One is {thing.Column_One}");
                    console.Output.WriteLine($">> Thing.Column_Two is {thing.Column_Two}");
                    console.Output.WriteLine($">> Thing.Column_Three is {thing.Column_Three}");

                    console.Output.WriteLine("---");
                }
            }
        }
    }
}
