/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('accountApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('accountCtrl', function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $menu) {
    $scope.$appUtil = $appUtil;
    $scope.Math = Math;

    $scope.init = function () {
        $scope.getUsers();
        $scope.getPricing('');
    }

    $scope.saveName = function () {
        $http.post('/Account/SaveName', { firstName: $scope.firstName, lastName: $scope.lastName }).then(function (res) { });
    }

    $scope.updateUserMemo = function (user) {
        $http.post('/Account/UpdateUserMemo/' + user.Id, { memo: user.Memo }).then(function (res) { });
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
            
        });
    }

    $scope.query = {
        order: 'UserCode',
        limit: 10,
        page: 1
    };


    //md-dialog
    $scope.showPricing = function (ev) {
        $mdDialog.show({
            controller: DialogController,
            templateUrl: 'pricing-dialog.html',
            scope: $scope,
            preserveScope: true,
            targetEvent: ev
        });
    };

    $scope.getTransactions = function (user) {
        $http.get('/Transaction/GetTransactions/' + (user ? user.UserCode : '')).then(function (res) {
            $scope.transactions = res.data;
        });
    }

    $scope.showTransactions = function (ev, user) {
        $scope.getTransactions(user);
        $mdDialog.show({
            controller: DialogController,
            templateUrl: 'transaction-dialog.html',
            scope: $scope,
            preserveScope: true,
            targetEvent: ev
        });
    };
    $scope.showDeposit = function (ev) {
        $mdDialog.show({
            controller: DialogController,
            templateUrl: 'deposit-dialog.html',
            scope: $scope,
            preserveScope: true,
            targetEvent: ev
        });
    };
    $scope.showManualDeposit = function (ev, user) {
        $scope.currentUser = user;
        $mdDialog.show({
            controller: DialogController,
            templateUrl: 'manual-deposit-dialog.html',
            scope: $scope,
            preserveScope: true,
            targetEvent: ev
        });
    };

    $scope.showPendingDeposits = function (ev, user) {
        $scope.currentUser = user;
        $http.get('/Transaction/GetDeposits/' + user.UserCode).then(function (res) {
            $scope.deposits = res.data;
        });
        $mdDialog.show({
            controller: DialogController,
            templateUrl: 'pending-deposits-dialog.html',
            scope: $scope,
            preserveScope: true,
            targetEvent: ev
        });
    }
    

    function DialogController($scope, $mdDialog) {
        $scope.close = function () {
            $mdDialog.cancel();
        };

        $scope.makeDeposit = function () {
            $http.post('/Transaction/Deposit', { deposit: $scope.deposit }).then(function (res) {
                $appUtil.appAlert(null, '', '充值已提交，金额会在24小时内到达您的账户余额');
                $mdDialog.cancel();
            });
        }

        $scope.confirmDeposit = function (deposit) {
            $http.post('/Transaction/ConfirmDeposit', { deposit: deposit }).then(function (res) {
                $scope.getUsers();
                $mdDialog.cancel();
            });
        }

        $scope.cancelDeposit = function (deposit) {
            $http.post('/Transaction/CancelDeposit', { deposit: deposit }).then(function (res) {
                $scope.getUsers();
                $mdDialog.cancel();
            });
        }

        $scope.manualDeposit = function (amount, memo, userCode) {
            $http.post('/Transaction/ManualDeposit', { amount: amount, memo: memo, userCode: userCode }).then(function (res) {
                $scope.getUsers();
                $mdDialog.cancel();
            });
        }
    }

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