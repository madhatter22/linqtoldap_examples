var linqtoldap = linqtoldap || {};
linqtoldap.viewModels = linqtoldap.viewModels || {};

linqtoldap.displayError = function(message) {
    $('#errorMessage').text(message);
    $("#general-error").show();
};

LinqToLdap.dataCall = function (url, data, onSuccess, onFail) {
    var success = onSuccess || function ()
    $.ajax({
        dataType: "json",
        url: url,
        data: data,
        success: success
    });
};