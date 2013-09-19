﻿linqtoldap.viewModels.UsersViewModel = function () {
    var self = this;

    var ListItemViewModel = function (item) {
        this.distinguishedName = ko.observable(item.DistinguishedName);
        this.name = ko.observable(item.Name);
        this.userId = ko.observable(item.UserId);
        
        this.load = function() {
            self.loadItem(this.userId());
        }.bind(this);
        
        return this;
    };

    var ItemViewModel = function () {
        this.distinguishedName = ko.observable('');
        this.commonName = ko.observable('');
        this.userId = ko.observable('');
        this.firstName = ko.observable('');
        this.lastName = ko.observable('');
        this.telephoneNumber = ko.observable('');

        this.setValues = function(item) {
            this.distinguishedName(item.DistinguishedName);
            this.commonName(item.CommonName);
            this.userId(item.UserId);
            this.firstName(item.FirstName);
            this.lastName(item.LastName);
            this.telephoneNumber(item.TelephoneNumber);
        };
    };

    self.isSearching = ko.observable(false);
    self.query = ko.observable('');
    self.mode = ko.observable('list');
    self.items = ko.observableArray([]);
    self.isCustomFilter = ko.observable(false);
    self.item = new ItemViewModel();

    self.showList = function() {
        self.mode('list');
    };

    self.loadItem = function (id) {
        alertify.log("Loading...", 'log', 1000);
        $.getJSON('/api/users/get', { id: id })
            .success(function (data) {
                self.item.setValues(data);
                self.mode('item');
            })
            .fail(function (jqxhr, textStatus, error) {
                alertify.error(textStatus + ': ' + error);
            });
    };
    
    self.search = function () {
        self.isSearching(true);
        alertify.log("Loading...", 'log', 1000);
        $.getJSON('/api/users/get', { q: self.query(), custom: self.isCustomFilter() })
            .success(function (data) {
                self.isSearching(false);
                self.items($.map(data, function(value) { return new ListItemViewModel(value); }));
            })
            .fail(function (jqxhr, textStatus, error) {
                self.isSearching(false);
                alertify.error(textStatus + ': ' + error);
            });
    };

    self.search();

    return self;
}