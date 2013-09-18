linqtoldap.viewModels.UsersViewModel = function () {
    var self = this;
    var nextPage;
    var currentFilter;

    var ListItemViewModel = function(item) {
        this.load = function() {
            alertify.log('placeholder for actually loading');
            self.mode('item');
        };

        this.distinguishedName = ko.observable(item.DistinguishedName);
        this.name = ko.observable(item.Name);

        return this;
    };

    self.query = ko.observable('');
    self.mode = ko.observable('list');
    self.items = ko.observableArray([]);
    self.canLoadMore = ko.observable(true);
    self.isCustomFilter = ko.observable(false);

    self.getPage = function () {
        alertify.log("Loading...", 'log', 1000);
        $.getJSON('/api/users/get', { q: self.query(), custom: self.isCustomFilter(), filter: currentFilter, nextPage: nextPage })
            .success(function (data) {
                var underlyingItems = self.items();
                $.each(data.Items, function(index, item) {
                    underlyingItems.push(new ListItemViewModel(item));
                });
                nextPage = data.NextPage;
                currentFilter = data.Filter;
                self.canLoadMore(nextPage && nextPage.length > 0);
                self.items.valueHasMutated();
            })
            .fail(function(jqxhr, textStatus, error) {
                alertify.error(textStatus + ' ' + error);
            });
    };

    self.search = function() {
        currentFilter = '';
        nextPage = '';
        self.getPage();
    };

    self.search();

    return self;
}