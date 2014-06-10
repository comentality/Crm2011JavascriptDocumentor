namespace CrmScriptLister
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    public class Parser
    {
        public string ToCSV(string xml)
        {
            var entities = this.Parse(xml);

            var line = new string[] {
                    "Entity", "Form type", "Event", "Field Name","Function Name", "Library Name", "Parameters", "Enabled", "PassExecutionConteaxt" 
                };

            var header = String.Join(", ", line);

            var docustring = new StringBuilder(header);

            docustring.AppendLine();

            foreach (var entity in entities)
            {
                line[0] = entity.EntityName;

                foreach (var trigger in entity.Triggers)
                {
                    line[1] = trigger.FormName;

                    foreach (var event_ in trigger.Handlers)
                    {
                        line[2] = event_.FunctionName;
                        line[3] = trigger.FieldName;

                        line[4] = event_.FunctionName;
                        line[5] = event_.LibraryName;
                        line[6] = event_.Parameters;
                        line[7] = event_.IsEnabled;
                        line[8] = event_.IsExecutionContextPassed;

                        docustring.AppendLine(String.Join(", ", line));
                    }
                }
            }

            return docustring.ToString();
        }

        public string ToTextile(string xml)
        {
            //StringBuilder docustring = new StringBuilder("h1. Javascript bindings on form");
            //docustring.AppendLine();
            //docustring.AppendLine();

            //foreach (var entity in entities)
            //{
            //    docustring.AppendFormat("h2. Entity: {0}", entity.entityName);
            //    docustring.AppendLine();
            //    docustring.AppendLine();


            //    foreach (var form in entity.Forms)
            //    {
            //        docustring.AppendFormat("h3. Form: {0}", form.Type);
            //        docustring.AppendLine();
            //        docustring.AppendLine();

            //        foreach (var event_ in form.Events)
            //        {
            //            docustring.AppendFormat("h4. {0}{1}", event_.EventName.Value, event_.FieldName != null ? " of " + event_.FieldName.Value : "");
            //            docustring.AppendLine();
            //            docustring.AppendLine();

            //            foreach (var hendler in event_.Handlers)
            //            {
            //                docustring.AppendLine("* " + hendler.FunctionName);
            //                docustring.AppendLine("* " + hendler.LibraryName);
            //                docustring.AppendLine("* " + hendler.Parameters);
            //                docustring.AppendLine("* " + hendler.IsEnabled);
            //                docustring.AppendLine("* " + hendler.IsExecutionContextPassed);
            //                docustring.AppendLine();
            //                docustring.AppendLine();
            //            }
            //        }
            //    }
            //}

            // TODO: textile
            return "NOT IMPLEMENTED YET.";
        }
        private List<EntityScriptList> Parse(string xml)
        {
            var xEl = XElement.Parse(xml);

            var anonchiki =
                xEl
                .Descendants("Entity")
                .Where(lda_entity => lda_entity.Descendants("form").Any())
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
                                    Type = lda_form.Ancestors("forms").Single().Attribute("type").Value,

                                    Name = string.Join(" ",
                                        lda_form.Ancestors("systemform").Single()
                                        .Element("LocalizedNames").IfNotNull(y =>
                                            y.Elements("LocalizedName").Attributes("description")
                                            .Select(x => x.Value))
                                            ),

                                    Events = lda_form
                                        .Descendants("event")
                                        .Where(lda_event => lda_event.Attribute("name").Value != "setadditionalparams")
                                        .Select(lda_event =>
                                            new
                                            {
                                                EventName = lda_event.Attribute("name"),
                                                FieldName = lda_event.Attribute("attribute").IfNotNull(x => x.Value),
                                                //IsActive = lda_event.Attribute("active"),
                                                Handlers =
                                                    lda_event
                                                    .Descendants("Handler")
                                                    .Select(lda_hendler =>
                                                        new
                                                        {
                                                            LibraryName = lda_hendler.Attribute("libraryName").Value,
                                                            FunctionName = lda_hendler.Attribute("functionName").Value,
                                                            IsEnabled = lda_hendler.Attribute("enabled").Value,
                                                            Parameters = lda_hendler.Attribute("parameters").IfNotNull(x => x.Value),
                                                            IsExecutionContextPassed = lda_hendler.Attribute("passExecutionContext").IfNotNull(x => x.Value)
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

            var entities = anonchiki.Select(entity =>
                new EntityScriptList
                {
                    EntityName = entity.entityName,

                    Triggers =
                        entity.Forms.SelectMany(form =>
                            form.Events.Select(evnt =>
                                new EntityScriptList.Trigger
                                {
                                    FormName = form.Name,
                                    FieldName = evnt.FieldName,
                                    Handlers = evnt.Handlers.Select(handler =>
                                        new EntityScriptList.Trigger.Handler
                                        {
                                            FunctionName = handler.FunctionName,
                                            IsEnabled = handler.IsEnabled,
                                            IsExecutionContextPassed = handler.IsExecutionContextPassed,
                                            LibraryName = handler.LibraryName,
                                            Parameters = handler.Parameters
                                        }).ToList()
                                }).ToList()
                        ).ToList()

                }).ToList();

            return entities;
        }
    }
}
