$(function () {

    var ul = $('#convert-status ul');

    ul.find("li").each(function () {

        let li = $(this);

        const curr_index = fileModelList.findIndex(x => x.name === li.attr("id"));
        console.log(curr_index);
        fileModelList[curr_index].status = "converting";

        let model = fileModelList[curr_index];

        li.find('i').html(`${formatFileSize(model.size)}`).change();

        li.find('input').knob();
        li.find('input').val(33).change();

        li.find('span').click(function () {
            //if (li.hasClass('working')) {
            //    jqXHR.abort();
            //}

            li.fadeOut(function () {
                for (var i = 0; i < fileModelList.length; i += 1) {
                    if (fileModelList[i].name == li.attr("id")) {
                        fileModelList.splice(i, 1);
                        break;
                    }
                }

                li.remove();

                if (fileModelList.length == 0) {
                    console.log("redirect to index");
                    window.location.replace(window.location.origin);
                }
                else {
                    console.log(fileModelList.filter(x => x.status !== "done").length);

                    if (fileModelList.filter(x => x.status !== "done").length == 0 &&
                        $('#download-button').prop('disabled') == true) {

                        $('#download-button').prop('disabled', false);
                    }
                }
            });
        });

        $.ajax({
            type: "POST",
            url: "/Home/Convert",
            data: {
                id: model.id,
                name: model.name,
                extension: model.extension,
                status: model.status
            },
            success: function (filename) {

                li.attr("id", filename);

                let p_text = li.find('p').html().split('\n');
                p_text[0] = filename;

                li.find('p').html(p_text.join('\n')).change();

                li.find('input').val(100).change();
                var progress = li.find('input').val();

                if (progress == 100) {

                    fileModelList[curr_index].name = filename;
                    fileModelList[curr_index].status = "done";

                    if (fileModelList.filter(x => x.status !== "done").length == 0 &&
                        $('#download-button').prop('disabled') == true) {

                        $('#download-button').prop('disabled', false);
                    }
                }
            }
        });
    });

    $('#download-button').click(function download() {
        if (fileModelList.filter(x => x.status === "done").length <= 0) {
            return;
        }

        const fileModel = fileModelList.filter(x => x.status === "done")[0];
        const curr_index = fileModelList.findIndex(x => x.status === "done");

        $.ajax({
            type: "POST",
            url: "/Home/Download",
            data: {
                filename: fileModel.name
            },
            xhrFields: {
                responseType: 'blob' // to avoid binary data being mangled on charset conversion
            },
            async: true,
            success: function (blob, status, xhr) {
                fileModelList[curr_index].status = "downloading";
                // check for a filename
                var filename = "";
                var disposition = xhr.getResponseHeader('Content-Disposition');
                if (disposition && disposition.indexOf('attachment') !== -1) {
                    var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                    var matches = filenameRegex.exec(disposition);
                    if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                }

                if (typeof window.navigator.msSaveBlob !== 'undefined') {
                    // IE workaround for "HTML7007: One or more blob URLs were revoked by closing the blob for which they were created. These URLs will no longer resolve as the data backing the URL has been freed."
                    window.navigator.msSaveBlob(blob, filename);
                } else {
                    var URL = window.URL || window.webkitURL;
                    var downloadUrl = URL.createObjectURL(blob);

                    if (filename) {
                        // use HTML5 a[download] attribute to specify filename
                        var a = document.createElement("a");
                        // safari doesn't support this yet
                        if (typeof a.download === 'undefined') {
                            window.location.href = downloadUrl;
                        } else {
                            a.href = downloadUrl;
                            a.download = filename;
                            document.body.appendChild(a);
                            a.click();
                        }
                    } else {
                        window.location.href = downloadUrl;
                    }

                    setTimeout(function () { URL.revokeObjectURL(downloadUrl); }, 100); // cleanup
                }

                $(`li[id="${filename}"] span`).click();
                if (fileModelList.filter(x => x.status === "done").length > 0)
                    download();
            }
        });
    });

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