using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MappingBJ
{
    class Logging
    {
        public static void Log( string entry = null, Exception ex = null, string application = null, 
                                string context = null, string ref1 = null, string ref2 = null, 
                                string ref3 = null)
        {
            Log log = new Log();

            log.Application = application;
            log.Context = context;
            log.Entry = entry;
            log.Ref1 = ref1;
            log.Ref2 = ref2;
            log.Ref3 = ref3;

            if (ex != null)
            {
                log.ExceptionMessage = ex.Message;
                log.StackTrace = ex.StackTrace;
            }

            log.Timestamp = DateTime.Now;

            ImportEntities ie = new ImportEntities();
            ie.Logs.Add(log);
            ie.SaveChanges();
        }
    }
}
