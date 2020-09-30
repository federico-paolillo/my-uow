using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using FP.UoW.Examples.ConsoleApplication.Services;

namespace FP.UoW.Examples.ConsoleApplication.Commands
{
    [Command("insert")]
    public sealed class InsertCommand : ICommand
    {
        private readonly ThingsService thingsService;

        public InsertCommand(ThingsService thingsService)
        {
            this.thingsService = thingsService ?? throw new ArgumentNullException(nameof(thingsService));
        }

        [CommandOption("count", IsRequired = true)]
        public int Count { get; set; }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            //Irrelevant, third party code
            var cancellationToken = console.GetCancellationToken();

            //We call the ThingsService here to insert random data
            await thingsService.InsertThingsAsync(Count, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}