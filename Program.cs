using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Crm2011JavascriptDocumentor
{
    class Program
    {
        public static bool ToTextile { get; set; }
        public static bool ToXsl { get; set; }
        public static bool ToCsv { get; set; }

        static void Main(string[] args)
        {
            ToCsv = true;
            ToTextile = false;
            ToXsl = false;

            string inputFilePath = args[0];
            //string inputFilePath = "customizations.xml";

            var xEl = XElement.Load(inputFilePath);

            var entities =
                xEl
                .Descendants("Entity")
                .Where(lda_entity => lda_entity.Descendants("form").Count() > 0)
                .Select(lda_entity =>
                    new
                    {
                        // entity name
                        entityName = lda_entity.Elements("Name").First().Value,

                        // form tree
                        Forms =
                            lda_entity
                            .Descendants("form")
                            .Where(lda_form =>
                                // exclude build-in uncustomizable forms
                                    (new List<string> { "mobile", "appointmentBook" })
                                    .TrueForAll(x =>
                                        x != lda_form.Ancestors("forms").Single().Attribute("type").Value))
                            .Select(lda_form =>
                                new
                                {
                                    Type = lda_form.Ancestors("forms").Single().Attribute("type"),

                                    Events = lda_form
                                        .Descendants("event")
                                        .Where(lda_event => lda_event.Attribute("name").Value != "setadditionalparams")
                                        .Select(lda_event =>
                                            new
                                            {
                                                EventName = lda_event.Attribute("name"),
                                                FieldName = lda_event.Attribute("attribute"),
                                                //IsActive = lda_event.Attribute("active"),
                                                Handlers =
                                                    lda_event
                                                    .Descendants("Handler")
                                                    .Select(lda_hendler =>
                                                        new
                                                        {
                                                            LibraryName = lda_hendler.Attribute("libraryName"),
                                                            FunctionName = lda_hendler.Attribute("functionName"),
                                                            IsEnabled = lda_hendler.Attribute("enabled"),
                                                            Parameters = lda_hendler.Attribute("parameters"),
                                                            IsExecutionContextPassed = lda_hendler.Attribute("passExecutionContext")
                                                        })
                                                    .ToList()
                                            }
                                        )
                                        .ToList()
                                })
                            .ToList()
                    }
                    )
                .ToList();

            if (ToTextile)
            {
                StringBuilder docustring = new StringBuilder("h1. Javascript bindings on form");
                docustring.AppendLine();
                docustring.AppendLine();

                foreach (var entity in entities)
                {
                    docustring.AppendFormat("h2. Entity: {0}", entity.entityName);
                    docustring.AppendLine();
                    docustring.AppendLine();


                    foreach (var form in entity.Forms)
                    {
                        docustring.AppendFormat("h3. Form: {0}", form.Type);
                        docustring.AppendLine();
                        docustring.AppendLine();

                        foreach (var event_ in form.Events)
                        {
                            docustring.AppendFormat("h4. {0}{1}", event_.EventName.Value, event_.FieldName != null ? " of " + event_.FieldName.Value : "");
                            docustring.AppendLine();
                            docustring.AppendLine();

                            foreach (var hendler in event_.Handlers)
                            {
                                docustring.AppendLine("* " + hendler.FunctionName);
                                docustring.AppendLine("* " + hendler.LibraryName);
                                docustring.AppendLine("* " + hendler.Parameters);
                                docustring.AppendLine("* " + hendler.IsEnabled);
                                docustring.AppendLine("* " + hendler.IsExecutionContextPassed);
                                docustring.AppendLine();
                                docustring.AppendLine();
                            }
                        }
                    }
                }

                System.IO.StreamWriter file = new System.IO.StreamWriter("js.textile");
                file.WriteLine(docustring.ToString());
                file.Close();
            }

            if (ToCsv)
            {
                string[] line = new string[] {
                    "Entity", "Form type", "Event", "Field Name","Function Name", "Library Name", "Parameters", "Enabled", "PassExecutionConteaxt" 
                };

                string header = String.Join(", ", line);

                StringBuilder docustring = new StringBuilder(header);

                docustring.AppendLine();

                foreach (var entity in entities)
                {
                    line[0] = entity.entityName;

                    foreach (var form in entity.Forms)
                    {
                        line[1] = form.Type.Value;

                        foreach (var event_ in form.Events)
                        {
                            line[2] = event_.EventName.Value;
                            line[3] = event_.FieldName != null ? event_.FieldName.Value : "";

                            foreach (var hendler in event_.Handlers)
                            {
                                line[4] = hendler.FunctionName.Value;
                                line[5] = hendler.LibraryName.Value;
                                line[6] = hendler.Parameters != null ? hendler.Parameters.Value : "";
                                line[7] = hendler.IsEnabled.Value;
                                line[8] = hendler.IsExecutionContextPassed != null ? hendler.IsExecutionContextPassed.Value : "";

                                docustring.AppendLine(String.Join(", ", line));
                            }
                        }
                    }
                }

                System.IO.StreamWriter file = new System.IO.StreamWriter("js.csv");
                file.WriteLine(docustring.ToString());
                file.Close();
            }
        }
    }
}
