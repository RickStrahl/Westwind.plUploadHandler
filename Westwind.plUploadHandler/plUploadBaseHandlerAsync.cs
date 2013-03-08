using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace Westwind.plUpload
{
    /// <summary>
    /// Same implementation as above but implements the Async request processsing.
    /// Quite useful in these scenarios since typical operations from file upload
    /// are IO operations - streaming to file or disk which tend to be relatively
    /// </summary>
    public abstract class plUploadBaseHandlerAsync : plUploadBaseHandler, IHttpAsyncHandler
    {
        #region Async Handler Implementation

        /// <summary>
        /// Delegate to ProcessRequest method
        /// </summary>
        Action<HttpContext> processRequest;

        public System.IAsyncResult BeginProcessRequest(HttpContext context, System.AsyncCallback cb, object extraData)
        {
            // call ProcessRequest method asynchronously            
            Task task = Task.Factory.StartNew(
                (ctx) => ProcessRequest(ctx as HttpContext),
                context);

            return task;               

        }

        public void EndProcessRequest(System.IAsyncResult result)
        {
            Task task = (Task)result;
            task.Wait();
        }
        # endregion
    }
}
