/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('packageApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('packageCtrl', ['$scope', '$http', '$filter', '$log', '$timeout', '$appUtil', '$mdDialog', '$menu',
    function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $menu) {
    $scope.$appUtil = $appUtil;
    
    $scope.startDate = new Date(new Date().getFullYear(), 0, 1);
    $scope.endDate = new Date();
    $scope.statusList = ['全部', '未预报', '待入库', '等待合箱', '待出库', '已出库', '空运', '清关中', '国内转运', '待退货', '已退货', '问题件'];
    $scope.status = $scope.statusList[0];
    $scope.receiver = '';
    $scope.packageCode = '';
    $scope.tracking = '';
    $scope.addressIDs = [];

    $scope.newPackageStatusList = ['待入库', '等待合箱'];
   

    $scope.selectedPackages = [];
    $scope.options = {
        rowSelection: true,
        multiSelect: true,
    };

    $scope.init = function () {
        $scope.getProductCategories();
        $log.log($scope.isAdmin);
        if ($scope.isAdmin) {
            $scope.newPackageStatusList = ['未预报', '待入库', '等待合箱', '待出库', '已出库', '空运', '清关中', '国内转运', '待退货', '已退货', '问题件'];
        }
    }

    $scope.searchPackages = function () {
        var model = {};
        model.startDate = $scope.startDate;
        model.endDate = $scope.endDate;
        model.status = $scope.status;
        model.receiver = $scope.receiver;
        model.packageCode = $scope.packageCode;
        model.tracking = $scope.tracking;
        model.userCode = $scope.userCode;
        model.userName = $scope.userName;
        $http.post('/Package/SearchPackages', { criteriaStr: JSON.stringify(model) }).then(function (res) {
            $scope.packages = res.data;
            $scope.packagesOrder = 'Code';
        });
    }

    $scope.refreshPackages = function () {
        $scope.startDate = new Date(new Date().getFullYear(), 0, 1);
        $scope.endDate = new Date();
        $scope.status = $scope.statusList[0];
        $scope.receiver = '';
        $scope.packageCode = '';
        $scope.tracking = '';
        $scope.searchPackages();
    }

    $scope.getPackage = function (code, callback) {
        $http.get('/Package/GetPackage/' + code).then(function (res) {
            $scope.package = res.data;
            if (callback) {
                callback();
            }
        });
    }

    $scope.deletePackage = function (code) {
        if (confirm('确认删除此包裹?')) {
            $http.get('/Package/DeletePackage/' + code).then(function (res) {
                $scope.refreshPackages();
            });
        }
    }

    $scope.returnPackage = function (code) {
        if (confirm('确认退货?')) {
            $scope.getPackage(code, function () {
                $scope.package.Status = '待退货';
                $scope.updatePackage();
            })
        }
    }

    $scope.addPackage = function () {
        var status = $scope.holdPackage ? '等待合箱' : '待入库';
        $scope.package = { Products: [{ Number: 1, Quantity: 1 }], Status: status, Address: {} };
        $scope.showPackageModal();
    };


    $scope.showPackageModal = function () {
        $('#packageModal').modal({
            backdrop: 'static',
            keyboard: false
        });
        $('#packageModal').modal('show');
        $scope.getAddresses();
    }

    //#region split package
    $scope.showSplitPackageModal = function () {
        $('#splitPackageModal').modal({
            backdrop: 'static',
            keyboard: false
        });
        $('#splitPackageModal').modal('show');
        $scope.newPackage = {};
        angular.forEach($scope.package, function (value, key) {
            if (key !== 'Code') {
                $scope.newPackage[key] = value;
            }
        });
        $scope.newPackage.Products = [];
    }

    $scope.splitProduct = function (index) {
        $scope.newPackage.Products.push($scope.package.Products[index]);
        $scope.package.Products.splice(index, 1);
    }

    $scope.cancelSplitProduct = function (index) {
        $scope.package.Products.push($scope.newPackage.Products[index]);
        $scope.newPackage.Products.splice(index, 1);
    }

    $scope.splitPackage = function () {
        $scope.updatePackage($scope.package);
        $scope.updatePackage($scope.newPackage);
    }
    //#endregion

    $scope.getProductCategories = function () {
        $http.post('/Package/GetProductCategories').then(function (res) {
            $scope.categories = res.data;
        });
    }

    $scope.addProduct = function () {
        $scope.package.Products.push({ Number: $scope.package.Products.length + 1, Quantity: 1 });
    }

    $scope.removeProduct = function (index) {
        $scope.package.Products.splice(index, 1);
        angular.forEach($scope.package.Products, function (value, key) {
            value.Number = $scope.package.Products.indexOf(value) + 1;
        });
    }

    $scope.updatePackage = function (package) {
        if (typeof package === 'undefined') {
            // update package
            package = $scope.package;

            angular.forEach(package.Products, function (value, key) {
                value.Tracking = package.Tracking;
                value.OrderNumber = package.OrderNumber;
                value.Notes = package.Notes;
            });
            $('#packageModal').modal('hide');
        }
        else {
            // update package after split
            package.Tracking = package.Products.map(function (product) { return product.Tracking; }).filter(function (value, index, self) { return self.indexOf(value) === index; }).join(';');
            package.OrderNumber = package.Products.map(function (product) { return product.OrderNumber; }).filter(function (value, index, self) { return self.indexOf(value) === index; }).join(';');
            package.Notes = package.Products.map(function (product) { return product.Notes; }).filter(function (value, index, self) { return self.indexOf(value) === index; }).join(';');
        }
        $http.post('/Package/UpdatePackage', { package: package }).then(function (res) {
            $scope.refreshPackages();
        });
    }

    // Address
    $scope.showAddressManager = function (newAddress) {
        $('#addressModal').modal({
            backdrop: 'static',
            keyboard: false
        });
        $('#addressModal').modal('show');
        if (newAddress) {
            $scope.package.Address = {};
        }
    }

    $scope.getDistricts = function (targetAttr, parentAttr, name) {
        var id = '';
        angular.forEach($scope[parentAttr], function (value, key) {
            if (name === value.Name) { id = value.ID; }
        });
        $http.get('/Address/GetDistricts/' + id).then(function (res) {
            $scope[targetAttr] = [];
            angular.forEach(res.data, function (value, key) {
                $scope[targetAttr].push({ ID: value.ID, Name: value.Name });
            });
        });
    }


    $scope.getAddresses = function () {
        if (typeof $scope.addresses === 'undefined' || $scope.addressIDs.indexOf($scope.package.Address.ID) === -1) {
            $http.post('/Address/GetAddresses/' + $scope.package.Address.ID).then(function (res) {
                $scope.addresses = res.data;
                if (typeof $scope.package.Address.ID === 'undefined') {
                    $scope.package.Address = $scope.addresses[0];
                }
                $scope.updateAddressString();
            });
        }
    }

    $scope.updateAddress = function () {
        $http.post('/Address/UpdateAddress', { address: $scope.package.Address }).then(function (res) {
            $scope.getAddresses();
        });
    }

    $scope.updateAddressString = function () {
        $scope.addressIDs = [];
        angular.forEach($scope.addresses, function (value, key) {
            value.addressString = value.ProvinceName + ' ' + value.CityName + ' ' + value.DistrictName + ' ' +
            value.Street + ' ' + value.Name + ' ' + value.Phone;
            $scope.addressIDs.push(value.ID);
        });
        $scope.provinces = [];
        $scope.provinces.push({ ID: $scope.package.Address.Province, Name: $scope.package.Address.ProvinceName });

        $scope.cities = [];
        $scope.cities.push({ ID: $scope.package.Address.City, Name: $scope.package.Address.CityName });

        $scope.districts = [];
        $scope.districts.push({ ID: $scope.package.Address.District, Name: $scope.package.Address.DistrictName });
    }

    $scope.deleteAddress = function (id) {
        if (confirm('确认删除此地址?')) {
            $http.get('/Address/DeleteAddress/' + id).then(function (res) {
                if (res.data !== '') { alert(res.data); }
                for (i = $scope.addresses.length - 1; i >= 0; i--) {
                    if ($scope.addresses[i].ID === id) {
                        $scope.addresses.splice(i, 1);
                        return;
                    }
                }
            });
        }
    }

    $scope.combinePackages = function () {
        var addressID = $scope.selectedPackages[0].AddressID;
        for (var i = 1; i < $scope.selectedPackages.length; i++) {
            if (addressID !== $scope.selectedPackages[i].AddressID) {
                alert('请确保每箱寄往同一地址再进行合箱操作。');
                return;
            }
            addressID = $scope.selectedPackages[i].AddressID;
        }
        if (confirm('确认进行合箱?')) {
            var packageCodes = [];
            angular.forEach($scope.selectedPackages, function (value, key) {
                packageCodes.push(value.Code);
            });
            $http.post('/Package/CombinePackages', { packageCodes: packageCodes }).then(function (res) {
                $scope.refreshPackages();
            });
        }
    }


    var vm = this;
    vm.$menu = $menu;
    vm.isOpen = function (section) {
        return $menu.isSectionSelected(section);
    };
    vm.toggleOpen = function (section) {
        $menu.toggleSelectSection(section);
    }
}]);