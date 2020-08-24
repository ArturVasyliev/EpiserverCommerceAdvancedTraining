using Mediachase.BusinessFoundation.Data;
using Mediachase.BusinessFoundation.Data.Business;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomBfHandler
{
    public class DemoPipelineHandler : BaseRequestHandler, IPlugin
    {
        private StringBuilder sb = new StringBuilder();
        public new void Execute(BusinessContext context)
        {
            switch (context.PluginStage)
            {
                case EventPipeLineStage.All:
                    break;
                case EventPipeLineStage.MainOperation:
                    break;
                case EventPipeLineStage.None:
                    break;
                case EventPipeLineStage.PostMainOperation:
                    PostExecWork(context);
                    break;
                case EventPipeLineStage.PostMainOperationInsideTranasaction:
                    break;
                case EventPipeLineStage.PreMainOperation:
                    PreExecWork(context);
                    break;
                case EventPipeLineStage.PreMainOperationInsideTranasaction:
                    break;
                default:
                    break;
            }
            base.Execute(context);
        }

        private void PreExecWork(BusinessContext context)
        {
            sb.AppendLine($"-- Pre Exec {DateTime.Now.ToLongTimeString()}--");

            var whatClass = context.GetTargetMetaClassName();
            sb.AppendLine($"The meta class is: {whatClass}");
            sb.AppendLine($"The method called: {context.GetMethod().ToString()}");

            PrimaryKeyId pk = (PrimaryKeyId)context.GetTargetPrimaryKeyId();
            EntityObject entity = BusinessManager.Load(whatClass, pk);
            sb.AppendLine($"The club card Title: {entity["TitleField"]}");
            sb.AppendLine("bruce was here");
            WriteValuesToLog();
        }

        private void PostExecWork(BusinessContext context)
        {
            sb.AppendLine($"-- Post Exec {DateTime.Now.ToLongTimeString()}--");

            var whatClass = context.GetTargetMetaClassName();
            sb.AppendLine($"The meta class is: {whatClass}");
            sb.AppendLine($"The method called: {context.GetMethod().ToString()}");

            PrimaryKeyId pk = (PrimaryKeyId)context.GetTargetPrimaryKeyId();
            EntityObject entity = BusinessManager.Load(whatClass, pk);
            sb.AppendLine($"The club card Title: {entity["TitleField"]}");

            WriteValuesToLog();
        }
        private void WriteValuesToLog()
        {
            using (StreamWriter outfile = new StreamWriter(@"C:\Episerver612\_CustomLogs\MyBfHandlerFile.txt", true))
            {
                outfile.Write(sb.ToString());
            }
        }
    }
}
