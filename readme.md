#Using the plUpload Component with ASP.NET Article Code
[Sample code for Blog Post ](http://www.west-wind.com/weblog/)

This project provides a base HTTP and HTTP Async Handler for capturing
plUpload based file uploads with ASP.NET. The base implementation provides 
a base handler, an async handler and a file output handler all of which 
can be subclassed. The handlers expose a few hook methods that you can simply
override to get notified when a file upload starts, each chunk arrives and
when it completes.

###Resources:###

* [plUpload Website](http://www.plupload.com/)
* [Using the plUpload Component Blog Post](http://www.west-wind.com/weblog)

### Sample Web Project Configuration###
The Web application sample is an image uploader that stores images to file.
If you plan on using a full version of IIS make sure you create the 
/tempUploads /UploadedImages folders in the web folder and give that
folder read/write/create access for the user account the IIS 
Application Pool is running under. You can find this account in the
your site/virtual's Application Pool's Advanced settings.

###How it works###
To use these classes use one of the provided base handlers and subclass
by creating a new Http Handler. The easiest way to do this is to create
an HTTP handler as ASHX handler. In the source code simply subclass
the ASHX handler 
