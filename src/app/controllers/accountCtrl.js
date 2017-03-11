/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('accountApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('accountCtrl', function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $menu) {
    $scope.$appUtil = $appUtil;

    $scope.init = function () {
        $scope.getUsers();
        $scope.getPricing('');
    }

    $scope.saveName = function () {
        $http.post('/Account/SaveName', { firstName: $scope.firstName, lastName: $scope.lastName }).then(function (res) { });
    }

    $scope.getUsers = function () {
        if ($scope.isAdmin) {
            $http.get('/Account/GetUsers').then(function (res) {
                $scope.users = res.data;
            });
        }
    }

    $scope.getPricing = function (userCode) {
        $scope.userCode = userCode;
        $http.get('/Account/GetPricing/' + userCode).then(function (res) {
            $scope.pricing = res.data;
        });
    }

    $scope.savePricing = function () {
        var pricing = [];
        angular.forEach($scope.pricing, function (value) {
            pricing.push({ Price: value.Price, UserCode: $scope.userCode, Channel: value.Channel });
        });
        $http.post('/Account/SavePricing', { pricing: pricing }).then(function (res) {
            $scope.pricing = res.data;
        });
    }

    $scope.query = {
        order: 'UserCode',
        limit: 10,
        page: 1
    };

    // SideNav
    var vm = this;
    vm.$menu = $menu;
    vm.isOpen = function (section) {
        return $menu.isSectionSelected(section);
    };
    vm.toggleOpen = function (section) {
        $menu.toggleSelectSection(section);
    }
});