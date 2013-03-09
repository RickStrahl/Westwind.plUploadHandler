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

        TaskCompletionSource<object> tcs;

        public System.IAsyncResult BeginProcessRequest(HttpContext context, System.AsyncCallback cb, object extraData)
        {
            tcs = new TaskCompletionSource<object>(context);                     
            
            // call ProcessRequest method asynchronously            
            var task = Task<object>.Factory.StartNew(
                (ctx) => {
                    ProcessRequest(ctx as HttpContext);
                    
                    if (cb != null)
                        cb(tcs.Task);

                    return null;
                },context)
            .ContinueWith(tsk =>
            {
                if (tsk.IsFaulted)
                    tcs.SetException(tsk.Exception);
                else
                    // Not returning a value, but TCS needs one so just use null
                    tcs.SetResult(null);

            },TaskContinuationOptions.ExecuteSynchronously);

            
            return tcs.Task;
        }

        public void EndProcessRequest(System.IAsyncResult result)
        {
        }
        # endregion
    }
}
