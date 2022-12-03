var fileList = [];

$(function () {

    var ul = $('#upload ul');

    $('#drop a').click(function(){
        // Simulate a click on the file input button
        // to show the file browser dialog
        $(this).parent().find('input').click();
    });

    $('#convert').on('submit', function () {

        $('#fileList').val(JSON.stringify(fileList));
        console.log($('#fileList').val());

        return true;
    });

    // Initialize the jQuery File Upload plugin
    $('#upload').fileupload({

        // This element will accept file drag/drop uploading
        dropZone: $('#drop'),

        // This function is called when a file is added to the queue;
        // either via the browse button, or via drag/drop:
        add: function (e, data) {

            var tpl = $('<li class="working"><input type="text" value="0" data-width="48" data-height="48"'+
                ' data-fgColor="#0788a5" data-readOnly="1" data-bgColor="#3e4043" /><p></p><div class="settings">'+
                '<label>Format: </label><select name="format" class="format-select"><option value="avi">avi</option>'+
                '<option value="mp4">mp4</option><option value="mov">mov</option></select></div><span></span></li>');

            // Append the file name and file size
            //var filename = data.files[0].name;
            tpl.find('p').text(data.files[0].name)
                         .append('<i>' + formatFileSize(data.files[0].size) + '</i>');

            // Add the HTML to the UL element
            data.context = tpl.appendTo(ul);

            // Initialize the knob plugin
            tpl.find('input').knob();

            // Listen for clicks on the cancel icon
            tpl.find('span').click(function(){

                if(tpl.hasClass('working')){
                    jqXHR.abort();
                }

                tpl.fadeOut(function () {
                    fileList = removeFileListItem(fileList, data.files[0].name);

                    tpl.remove();

                    if (fileList.length == 0) {
                        $('#convert-button').prop('disabled', true);
                    }
                });

            });

            var select = data.context.find('.format-select');
            select.change(function () {
                fileList[fileList.findIndex(x => x.key == data.files[0].name)].val = select.find('option:selected').text();

                console.log('format changed')
            });

            // Automatically upload the file once it is added to the queue
            var jqXHR = data.submit();
        },

        progress: function(e, data){

            // Calculate the completion percentage of the upload
            var progress = parseInt(data.loaded / data.total * 100, 10);

            // Update the hidden input field and trigger a change
            // so that the jQuery knob plugin knows to update the dial
            data.context.find('input').val(progress).change();

            if(progress == 100){
                data.context.removeClass('working');
                var select = data.context.find('.format-select option:selected').text();
                var fileName = data.files[0].name;

                fileList.push({
                    key: fileName,
                    value: select
                });

                if ($('#convert-button').prop('disabled') == true) {
                    $('#convert-button').prop('disabled', false);
                }
            }
        },

        fail:function(e, data){
            // Something has gone wrong!
            data.context.addClass('error');

            delete fileList[data.files[0].name];

            if (fileList.length == 0) {
                $('#convert-button').prop('disabled', true);
            }

            setTimeout(() => {
                const elem = data.context;
                elem.remove();
            }, 2000);
        }

    });


    // Prevent the default action when a file is dropped on the window
    $(document).on('drop dragover', function (e) {
        e.preventDefault();
    });


    // Helper function to remove item from fileList
    function removeFileListItem(list, item) {
        for (var i = 0; i < list.length; i += 1) {
            if (list[i].key == item) {
                list.splice(i, 1);
                break;
            }
        }
        return list;
    }


    // Helper function that formats the file sizes
    function formatFileSize(bytes) {
        if (typeof bytes !== 'number') {
            return '';
        }

        if (bytes >= 1000000000) {
            return (bytes / 1000000000).toFixed(2) + ' GB';
        }

        if (bytes >= 1000000) {
            return (bytes / 1000000).toFixed(2) + ' MB';
        }

        return (bytes / 1000).toFixed(2) + ' KB';
    }

});