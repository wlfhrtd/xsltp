using System.Text;
using System.Xml;
using System.Xml.Xsl;

internal static class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 ||
                args.Contains("--help") ||
                args.Contains("-h"))
            {
                PrintHelp();
                return 0;
            }

            string? xmlPath = null;
            string? xsltPath = null;

            bool consoleOnly = false;
            bool prettyPrint = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--xml":

                        if (i + 1 >= args.Length)
                        {
                            Console.Error.WriteLine(
                                "--xml requires a file path."
                            );

                            return 1;
                        }

                        xmlPath = args[++i];

                        break;

                    case "--xslt":

                        if (i + 1 >= args.Length)
                        {
                            Console.Error.WriteLine(
                                "--xslt requires a file path."
                            );

                            return 1;
                        }

                        xsltPath = args[++i];

                        break;

                    case "--console-only":
                        consoleOnly = true;
                        break;

                    case "--pretty":
                        prettyPrint = true;
                        break;

                    default:

                        Console.Error.WriteLine(
                            $"Unknown option: {args[i]}"
                        );

                        return 1;
                }
            }

            if (xmlPath == null || xsltPath == null)
            {
                Console.Error.WriteLine(
                    "Missing required arguments."
                );

                PrintHelp();

                return 1;
            }

            if (!File.Exists(xmlPath))
            {
                Console.Error.WriteLine(
                    $"XML file not found: {xmlPath}");

                return 1;
            }

            if (!File.Exists(xsltPath))
            {
                Console.Error.WriteLine(
                    $"XSLT file not found: {xsltPath}");

                return 1;
            }

            Transform(
                xmlPath,
                xsltPath,
                consoleOnly,
                prettyPrint
            );

            return 0;
        }
        catch (XsltException ex)
        {
            Console.Error.WriteLine(
                $"XSLT ERROR Line={ex.LineNumber} Pos={ex.LinePosition}");

            Console.Error.WriteLine(ex.Message);

            if (ex.InnerException != null)
            {
                Console.Error.WriteLine(
                    $"INNER: {ex.InnerException.Message}");
            }

            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                ex.Message
            );

            return 3;
        }
    }

    static void Transform(
        string xmlPath,
        string xsltPath,
        bool consoleOnly,
        bool prettyPrint)
    {
        var resolver = new XmlUrlResolver();

        var transform =
            new XslCompiledTransform();

        var xsltSettings =
            new XsltSettings(
                enableDocumentFunction: false,
                enableScript: false
            );

        transform.Load(
            xsltPath,
            xsltSettings,
            resolver
        );

        var outputMethod =
            transform
            .OutputSettings
            .OutputMethod;

        var settings =
            transform.OutputSettings.Clone();

        if (prettyPrint)
        {
            settings.Indent = true;
            settings.IndentChars = "    ";
        }

        var readerSettings =
            new XmlReaderSettings
            {
                DtdProcessing =
                    DtdProcessing.Prohibit
            };

        using var xmlReader =
            XmlReader.Create(
                xmlPath,
                readerSettings
            );

        if (consoleOnly)
        {
            // disable BOM
            Console.OutputEncoding = Encoding.UTF8;
            settings.Encoding =
                new UTF8Encoding(false);

            var stdout = Console.OpenStandardOutput();

            using var writer =
                XmlWriter.Create(
                    stdout,
                    settings
                );

            transform.Transform(
                xmlReader,
                null,
                writer,
                resolver
            );

            writer.Flush();
        }
        else
        {
            var outputPath =
                BuildOutputPath(
                    xmlPath,
                    outputMethod
                );

            using var stream =
                File.Create(outputPath);

            using var writer =
                XmlWriter.Create(
                    stream,
                    settings
                );

            transform.Transform(
                xmlReader,
                null,
                writer,
                resolver
            );

            writer.Flush();

            Console.WriteLine(
                $"Saved: {outputPath}"
            );
        }
    }

    static string BuildOutputPath(
        string inputPath,
        XmlOutputMethod method)
    {
        var dir =
            Path.GetDirectoryName(inputPath)
            ?? Environment.CurrentDirectory;

        var name =
            Path.GetFileNameWithoutExtension(
                inputPath
            );

        string ext =
            method ==
            XmlOutputMethod.Text
            ? ".txt"
            : ".xml";

        return Path.Combine(
            dir,
            $"{name}_transformed{ext}"
        );
    }

    static void PrintHelp()
    {
        Console.WriteLine(
@"xsltp - XSLT Processor

Usage:

xsltp.exe
  --xml input.xml
  --xslt transform.xslt

Options:

--console-only
    Print only to console.

--pretty
    Pretty-print.

-h
--help
    Get some help."
        );
    }
}