linqtoldap.viewModels.ServerInfoViewModel = function () {
    var self = this;
    self.serverSettings = ko.observableArray([]);

    var ListItemViewModel = function (item) {
        return {
            key: item.Key,
            value: item.Value
        };
    };

    alertify.log("Loading...", 'log', 1000);
    $.getJSON('/api/serverinfo', function (data) {
        self.serverSettings($.map(data, function (item) { return new ListItemViewModel(item); }));
    })
    .fail(function(jqxhr, textStatus, error) {
        alertify.error(textStatus + ' ' + error);
    });
    
    return self;
};