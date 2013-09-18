linqtoldap.viewModels.ServerInfoViewModel = function () {
    var self = this;
    self.serverSettings = ko.observableArray([]);

    var listItemViewModel = function (item) {
        return {
            key: item.Key,
            value: item.Value
        };
    };

    alertify.log("Loading...", 'log', 1000);
    $.getJSON('/api/serverinfo', function (data) {
        var array = [];
        $.each(data, function(index, item) {
            array.push(new listItemViewModel(item));
        });
        self.serverSettings(array);
    })
    .fail(function(jqxhr, textStatus, error) {
        alertify.error(textStatus + ' ' + error);
    });
    
    return self;
};