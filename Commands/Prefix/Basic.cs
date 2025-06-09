using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lytheria.commands
{
    public class Basic : BaseCommandModule
    {
        [Command("calculate")]
        public async Task CommandParameterExample(CommandContext ctx, int num1, string operation, int num2)
        {
            int result = 0;

            switch (operation)
            {
                case "+":
                    result = num1 + num2;
                    break;
                case "-":
                    result = num1 - num2;
                    break;
                case "*":
                    result = num1 * num2;
                    break;
                case "/":
                    if (num2 == 0)
                    {
                        await ctx.Channel.SendMessageAsync("Cannot divide by zero.");
                        return;
                    }
                    result = num1 / num2;
                    break;
                default:
                    await ctx.Channel.SendMessageAsync("Invalid operation. Use +, -, *, or /.");
                    return;
            }

            await ctx.Channel.SendMessageAsync($"Result: {result}");
        }
    }
}
