$(document).ready(function () {
    // initialize status bar
    showStatus({ autoClose: true });

    $("#Uploader").pluploadQueue({
        runtimes: 'html5,silverlight,flash,html4',
        url: 'ImageUploadHandler.ashx',
        max_file_size: '2mb',
        chunk_size: '64kb',
        unique_names: false,
        // Resize images on clientside if we can
        resize: { width: 800, height: 600, quality: 90 },
        // Specify what files to browse for
        filters: [{ title: "Image files", extensions: "jpg,jpeg,gif,png" }],
        flash_swf_url: 'scripts/plupload/plupload.flash.swf',
        silverlight_xap_url: 'scripts/plupload/plupload.silverlight.xap',
        multiple_queues: true
    });

    // get uploader instance
    var uploader = $("#Uploader").pluploadQueue();      


    $("#btnStopUpload").click(function () {
        uploader.stop();
    });
    $("#btnStartUpload").click(function () {
        uploader.start();
    });

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
                            var ip = $("#ImagePreview");
                            
                            // show as overlay
                            ip.fadeIn("slow")
                              .modalDialog()
                              .closable()
                              .draggable();

                            // close the modal by clicking on the overlay
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

    // only allow 5 files to be uploaded at once
    uploader.bind("FilesAdded", function (up, filesToBeAdded) {
        if (up.files.length > 5) {
            up.files.splice(4, up.files.length - 5);
            showStatus("Only 5 files max are allowed per upload. Extra files removed.", 3000, true);
            return false;
        }
        return true;
    });

    //The plUpload labels are not customizable explicitly
    //so if you want to do this you have to directly manipulate the DOM
    setTimeout(function () {
        $(".plupload_header_title").text("Upload Images")
        $(".plupload_header_text").html("Add images to upload and click start. Images are resized to 600 pixels height and can't be larger than 1 meg.")
    }, 200);

});


