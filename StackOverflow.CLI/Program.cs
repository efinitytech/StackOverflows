using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using StackOverflow;

namespace ResizeImage
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RootCommand command =
                new RootCommand("A bunch of cli commands using code I copied from www.stackoverflow.com")
                {
                    ResizeImageCommand(),
                    MyConsole.GetVerbosityOption()
                };
            command.Invoke(args);
        }

        private static Command ResizeImageCommand()
        {
            Command command = new Command("ResizeImage", "Save an image at a different size")
            {
                MyConsole.GetVerbosityOption(),
                new Option("--file", "The destination of the input image file to resize.")
                {
                    Argument = new Argument<FileInfo>("Location").ExistingOnly()
                },
                new Option("--output", "The destination for the output file (original extension will be appended).")
                {
                    Argument = new Argument<string>("Location")
                },
                new Option("--width", "The new width dimension. Required if --height is omitted.")
                {
                    Argument = new Argument<int>()
                },
                new Option("--height", "The new height dimension. Required if --width is omitted.")
                {
                    Argument = new Argument<int>()
                },
                new Option("--overwrite", @"When true, overwrites the existing file rather than creating a copy.
Cannot be used with --output.")
                {
                    Argument = new Argument<bool>() {
                        Arity = ArgumentArity.ZeroOrOne,
                    },
                },
                new Option("--quality", @"The compression quality to apply to the new image.
Defaults to 100.")
                {
                    Argument = new Argument<long>()
                }
            };
            command.Handler = CommandHandler.Create((
                FileInfo file,
                string output,
                int width,
                int height,
                long quality,
                bool overwrite,
                int v) =>
            {
                MyConsole.Verbosity = v;
                MyConsole.Debug($"Verbosity set to {v}");
                if (overwrite && output != default)
                {
                    MyConsole.Error("Cannot use --overwrite with --output");
                    return;
                }
                if (quality == default)
                {
                    quality = 100L;
                }
                ResizeImage.Exec(file, output, width, height, quality, overwrite);
            });
            return command;
        }
    }
}
