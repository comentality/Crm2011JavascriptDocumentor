namespace CrmScriptLister
{
    using System.Collections.Generic;

    public class EntityScriptList
    {
        public string EntityName { get; set; }

        public List<Trigger> Triggers { get; set; }

        public class Trigger
        {
            public string FormName { get; set; }

            public string FieldName { get; set; }

            public List<Handler> Handlers { get; set; }

            public class Handler
            {
                public string LibraryName { get; set; }

                public string FunctionName { get; set; }

                public string IsEnabled { get; set; }

                public string Parameters { get; set; }

                public string IsExecutionContextPassed { get; set; }
            }
        }
    }
}
