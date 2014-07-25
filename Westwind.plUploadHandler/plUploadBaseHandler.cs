using System;
using System.IO;
using System.Web;
using plUploadHandler.Properties;
using System.Web.Script.Serialization;

namespace Westwind.plUpload
{

/// <summary>
/// Base implementation of the plUpload HTTP Handler.
/// 
/// The base handler doesn't handle storage in any way
/// it simply gets message event methods fired when 
/// the download is started, when a chunk arrives and when 
/// the download is completed.
/// 
/// This abstract class should be subclassed to do something
/// with the received chunks like stream them to disk 
/// or a database.
/// </summary>
public abstract class plUploadBaseHandler : IHttpHandler
{
    // Don't use HttpContext.Current - Async handlers don't see it
    protected HttpContext Context;
    protected HttpResponse Response;
    protected HttpRequest Request;

    public bool IsReusable
    {
        get { return false; }
    }

    /// <summary>
    /// Maximum upload size in bytes
    /// default: 0 = unlimited
    /// </summary>
    protected int MaxUploadSize = 0;

    /// <summary>
    /// Comma delimited list of extensions allowed,
    /// extension preceded by a dot.
    /// Example: .jpg,.png
    /// </summary>
    protected string AllowedExtensions = ".jpg,.jpeg,.png,.gif,.bmp";


    public void ProcessRequest(HttpContext context)
    {
        Context = context;
        Request = context.Request;
        Response = context.Response;

        // Check to see whether there are uploaded files to process them
        if (Request.Files.Count > 0)
        {
            HttpPostedFile fileUpload = Request.Files[0];

            string fileName = fileUpload.FileName;
            if (string.IsNullOrEmpty(fileName) || string.Equals(fileName,"blob"))
                fileName = Request["name"] ?? string.Empty;

            // normalize file name to avoid directory traversal attacks            
            fileName = Path.GetFileName(fileName);

            // check for allowed extensions and block
            string ext = Path.GetExtension(fileName);
            if (!("," + AllowedExtensions.ToLower() + ",").Contains("," + ext.ToLower() + ","))
            {
                WriteErrorResponse(Resources.InvalidFileExtensionUploaded);
                return;
            }

            string tstr = Request["chunks"] ?? string.Empty;
            int chunks = -1;
            if (!int.TryParse(tstr, out chunks))
                chunks = -1;
            tstr = Request["chunk"] ?? string.Empty;
            int chunk = -1;
            if (!int.TryParse(tstr, out chunk))
                chunk = -1;

            // If there are no chunks sent the file is sent as one 
            // this likely a plain HTML 4 upload (ie. 1 single file)
            if (chunks == -1)
            {
                if (MaxUploadSize == 0 || Request.ContentLength <= MaxUploadSize)
                {
                    if (!OnUploadChunk(fileUpload.InputStream, 0, 1, fileName))
                        return;
                }
                else
                {
                    WriteErrorResponse(Resources.UploadedFileIsTooLarge, 413);
                    return;
                }

                OnUploadCompleted(fileName);

                return;
            }
            else
            {
                // this isn't exact! We can't see the full size of the upload
                // and don't know the size of the last chunk
                if (chunk == 0 && MaxUploadSize > 0 && Request.ContentLength * (chunks - 1) > MaxUploadSize)
                    WriteErrorResponse(Resources.UploadedFileIsTooLarge, 413);
            }

            if (!OnUploadChunkStarted(chunk, chunks, fileName))
                return;

            // chunk 0 is the first one
            if (chunk == 0)
            {
                if (!OnUploadStarted(chunk, chunks, fileName))
                    return;
            }

            if (!OnUploadChunk(fileUpload.InputStream, chunk, chunks, fileName))
                return;

            // last chunk
            if (chunk >= chunks - 1)
            {
                // final response should just return
                // the output you generate
                OnUploadCompleted(fileName);
                return;
            }

            // if no response has been written yet write a success response
            WriteSucessResponse();
        }
    }


    /// <summary>
    /// Writes out an error response
    /// </summary>
    /// <param name="message"></param>
    /// <param name="statusCode"></param>
    /// <param name="endResponse"></param>
    protected void WriteErrorResponse(string message, int statusCode = 100, bool endResponse = false)
    {
        Response.ContentType = "application/json";
        Response.StatusCode = 500;

        // Write out raw JSON string to avoid JSON requirement
        Response.Write("{\"jsonrpc\" : \"2.0\", \"error\" : {\"code\": " + statusCode.ToString() + ", \"message\": " + JsonEncode(message) + "}, \"id\" : \"id\"}");
        if (endResponse)
            Response.End();
    }

    /// <summary>
    /// Sends a message to the client for each chunk
    /// </summary>
    /// <param name="message"></param>
    protected void WriteSucessResponse(string message = null)
    {
        Response.ContentType = "application/json";
        string json = null;
        if (!string.IsNullOrEmpty(message))
            json = JsonEncode(message);
        else
            json = "null";

        Response.Write("{\"jsonrpc\" : \"2.0\", \"result\" : " + json + ", \"id\" : \"id\"}");
    }

    /// <summary>
    /// Use this method to write the final output in the OnUploadCompleted method
    /// to pass back a result string to the client when a file has completed
    /// uploading
    /// </summary>
    /// <param name="data"></param>
    protected void WriteUploadCompletedMessage(string data)
    {
        Response.Write(data);
    }

    /// <summary>
    /// Completion handler called when the download completes
    /// </summary>
    /// <param name="fileName"></param>
    protected virtual void OnUploadCompleted(string fileName)
    {

    }

    /// <summary>
    /// Fired on every chunk that is sent
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="chunks"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    protected virtual bool OnUploadChunkStarted(int chunk, int chunks, string fileName)
    {
        return true;
    }

    /// <summary>
    /// Fired on the first chunk sent to the server - allows checking for authentication
    /// file size limits etc.
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="chunks"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    protected virtual bool OnUploadStarted(int chunk, int chunks, string fileName)
    {
        return true;
    }

    /// <summary>
    /// Fired as the upload happens
    /// </summary>
    /// <param name="chunkStream"></param>
    /// <param name="chunk"></param>
    /// <param name="chunks"></param>
    /// <param name="name"></param>
    /// <returns>return true on success false on failure</returns>
    protected virtual bool OnUploadChunk(Stream chunkStream, int chunk, int chunks, string fileName)
    {
        return true;
    }

    /// <summary>
    /// Encode JavaScript
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected string JsonEncode(object value)
    {
        var ser = new JavaScriptSerializer();
        return ser.Serialize(value);
    }
}
}
