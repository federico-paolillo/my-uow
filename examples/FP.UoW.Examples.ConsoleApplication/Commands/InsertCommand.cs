using CliFx;
using CliFx.Attributes;
using FP.UoW.Examples.ConsoleApplication.Services;
using System;
using System.Threading.Tasks;

namespace FP.UoW.Examples.ConsoleApplication.Commands
{
    [Command("insert")]
    public sealed class InsertCommand : ICommand
    {
        [CommandOption("count", IsRequired = true)]
        public int Count { get; set; }

        private readonly ThingsService thingsService = null;

        public InsertCommand(ThingsService thingsService)
        {
            this.thingsService = thingsService ?? throw new ArgumentNullException(nameof(thingsService));
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            //Irrelevant, third party code
            var cancellationToken = console.GetCancellationToken();

            //We call the ThingsService here to insert random data
            await thingsService.InsertThingsAsync(Count, cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
