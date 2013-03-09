#Using the plUpload Component with ASP.NET Article Code
[Sample code for Blog Post ](http://www.west-wind.com/weblog/posts/2013/Mar/12/Using-plUpload-to-upload-Files-with-ASPNET)

This project provides a base HTTP and HTTP Async Handler for capturing
plUpload based file uploads with ASP.NET. The base implementation provides 
a base handler, an async handler and a file output handler all of which 
can be subclassed. The handlers expose a few hook methods that you can simply
override to get notified when a file upload starts, each chunk arrives and
when it completes.

###Resources:###

* [plUpload Website](http://www.plupload.com/)
* [Using the plUpload Component Blog Post](http://www.west-wind.com/weblog/posts/2013/Mar/12/Using-plUpload-to-upload-Files-with-ASPNET)

### Sample Web Project Configuration###
The Web application sample is an image uploader that stores images to file.
If you plan on using a full version of IIS make sure you create the 
/tempUploads /UploadedImages folders in the web folder and give that
folder read/write/create access for the user account the IIS 
Application Pool is running under. You can find this account in the
your site/virtual's Application Pool's Advanced settings.

###How to implement a plUpload Image Uploader Example###
If you want the long version of this example, go check the blog post - 
there's a lot more info. Here is just a short summary of what you need to know.

plUpload is a client component. This library provides the server side
component that receives the clients upload chunks.

In order to use plUpload you'll need some JavaScript code fired from your 
HTML page. There's a plain uploader that is non-visual uploader that 
can handle the process of uploading files and providing progress events. 
There are also a couple jQuery components that provide the UI for selecting 
files and showing progress information.

The following example uses the plUpload jQueryQueryQueue component to upload
images. The client code might look like this:

```javascript
// set up the uploader queue
$("#Uploader").pluploadQueue({
    runtimes: 'html5,silverlight,flash,html4',   
    url: 'ImageUploadHandler.ashx',
    max_file_size: '1mb',
    chunk_size: '100kb',
    unique_names: false,
    // Resize images on clientside if we can
    resize: { width: 800, height: 600, quality: 90 },
    // Specify what files to browse for
    filters: [{ title: "Image files", extensions: "jpg,jpeg,gif,png" }],
    flash_swf_url: 'scripts/plupload/plupload.flash.swf',
    silverlight_xap_url: 'scripts/plupload/plupload.silverlight.xap',
    multiple_queues: true,
});

// get uploader instance
var uploader = $("#Uploader").pluploadQueue();      

// bind uploaded event and display the image
// response.response returns the last response from server
// which is the URL to the image that was sent by OnUploadCompleted
uploader.bind("FileUploaded", function (upload, file, response) {
    // remove the file from the list
    upload.removeFile(file);

    // Response.response returns server output from onUploadCompleted
    // our code returns the url to the image so we can display it
    var imageUrl = response.response;

    $("<img>").attr({ src: imageUrl })
              .click(function () {
                  $("#ImageView").attr("src", imageUrl);
                  setTimeout(function () {
                      $("#ImagePreview").modalDialog()
                                        .closable()
                                        .draggable();
                      $("#_ModalOverlay").click(function () {
                          $("#ImagePreview").modalDialog("hide");
                      });
                  }, 200);
              })
              .appendTo($("#ImageContainer"));
});

// Error handler displays client side errors and transfer errors
// when you click on the error icons
uploader.bind("Error", function (upload, error) {
    showStatus(error.message,3000,true);
});
```

This code sets up the visual component and specifies that plUpload should
use Html5 first and then use the silverlight and flash components and finally
fall back to plain HTML uploads.

Note that you probably want to handle the *FileUploaded* event to do something
in response to the uploaded image. In the above example, the uploaded
image is displayed in the UI image list, so as images are uploaded they
show up immediately.

The client plUpload component works by sending small chunks of data along
with some basic information about the uploaded file and chunk that's being
sent via Multi-part HTML forms.

###Server Side###
The server side is responsible for capturing the multi-part POST data
that the plUpload component sends and that's the task of this small library.

To use these classes use one of the provided base handlers and subclass
by creating a new Http Handler. The easiest way to do this is to create
an HTTP handler as ASHX handler. 

In the source code simply subclass the ASHX handler from one of the provided handlers. 
Typically you'll only need to implement the OnUploadCompleted() method to handle
the (or other data source into which the data was loaded), to do something 
with the completed data.

In this example, the uploaded image is resized and re-written to a separate 
folder with a new name. The OnUploadCompleted() method should return an
HTTP response that the client can use via the response.response parameter
shown in the FileUploaded client script. Here the result written is simply
the full URL to the newly uploaded and resized image:

```C#
public class ImageUploadHandler : plUploadFileHandler
{
    const string ImageStoragePath = "~/UploadedImages";        
    public static int ImageHeight = 480;

    public ImageUploadHandler()
    {
        // Normally you'd set these values from config values
        FileUploadPhysicalPath = "~/tempuploads";
        MaxUploadSize = 2000000;
    }       

    protected override void OnUploadCompleted(string fileName)
    {
        var Server = Context.Server;

        // Physical Path is auto-transformed
        var path = FileUploadPhysicalPath;
        var fullUploadedFileName = Path.Combine(path, fileName);

            
        var ext = Path.GetExtension(fileName).ToLower();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif")            
        {
            WriteErrorResponse("Invalid file format uploaded.");
            return;
        }

        // Typically you'd want to ensure that the filename is unique
        // Some ID from the database to correlate - here I use a static img_ prefix
        string generatedFilename = "img_" + fileName;
            
        try
        {
            // resize the image and write out in final image folder
            ResizeImage(fullUploadedFileName, Server.MapPath("~/uploadedImages/"+ generatedFilename), ImageHeight);
                
            // delete the temp file
            File.Delete(fullUploadedFileName);
        }
        catch(Exception ex)
        {
            WriteErrorResponse("Unable to write out uploaded file: " + ex.Message);
            return;
        }

        string finalImageUrl = Request.ApplicationPath + "/uploadedImages/" + generatedFilename;

        // return something that makes sense to your front-end UI
        // here I return the URL to the image.
        Response.Write(finalImageUrl);
    }
}
```

That's all that's needed... 

The OnUploadCompleted() should Response.Write

if there's an error you want to return, call WriteErrorResponse() with
an error message, which is then sent to the client which can echo the method.