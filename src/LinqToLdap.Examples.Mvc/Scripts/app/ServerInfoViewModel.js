linqtoldap.viewModels.serverInfoViewModel = (function () {
    var self = this;
    var keyValueViewModel = function (item) {
        return {
            key: item.Key,
            value: item.Value
        };
    };
    
    self.serverSettings = ko.observableArray([]);

    $.getJSON('Api/ServerInfo/Get', function(data) {
        var array = [];
        $.each(data, function(index, item) {
            array.push(new keyValueViewModel(item));
        });
        self.serverSettings(array);
    })
    .fail(function(jqxhr, textStatus, error) {
        linqtoldap.displayError(textStatus + ' ' + error);
    });

    return self;
})();