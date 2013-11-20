linqtoldap.viewModels.ServerInfoViewModel = function () {
    var self = this;
    self.serverSettings = ko.observableArray([]);

    var ListItemViewModel = function (item) {
        return {
            key: item.Key,
            value: item.Value
        };
    };

    $.blockUI();
    $.getJSON('/api/serverinfo')
        .success(function (data) {
            $.unblockUI();
            self.serverSettings($.map(data, function(item) { return new ListItemViewModel(item); }));
        })
        .fail(function (jqxhr, textStatus, error) {
            $.unblockUI();
            alertify.error(textStatus + ': ' + error);
        });
    
    return self;
};