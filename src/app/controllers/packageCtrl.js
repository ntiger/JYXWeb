/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('packageApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']).config(['$httpProvider', function ($httpProvider) {
            if (!$httpProvider.defaults.headers.get) {
                $httpProvider.defaults.headers.get = {};
            }
            $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
            $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
            $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';

            if (!$httpProvider.defaults.headers.post) {
                $httpProvider.defaults.headers.post = {};
            }
            $httpProvider.defaults.headers.post['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
            $httpProvider.defaults.headers.post['Cache-Control'] = 'no-cache';
            $httpProvider.defaults.headers.post['Pragma'] = 'no-cache';
        }]);
}
angularApp.controller('packageCtrl', ['$scope', '$http', '$filter', '$log', '$timeout', '$appUtil', '$mdDialog', '$fileReader', '$menu',
    function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $fileReader, $menu) {
        $scope.$appUtil = $appUtil;
        var d = new Date();
        d.setMonth(d.getMonth() - 2);
        $scope.startDate = new Date(d.getFullYear(), d.getMonth(), 1);
        $scope.endDate = new Date();
        $scope.statusList = ['全部', '待入库', '已入库', '已出库', '空运中', '清关中', '国内转运', '待退货', '已退货', '问题件'];
        $scope.statusListAdmin = ['全部', '待入库', '确认发货(未到货)', '已入库', '未预报', '确认发货(已到货)', '已出库', '空运中', '清关中', '国内转运', '待退货', '已退货', '问题件'];
        $scope.channelList = ['身份证渠道', '无身份证渠道', '混装渠道'];
        $scope.statusDict = {};
        $scope.exportTemplates = ['默认格式', '实重收费格式', '混装格式'];
        $scope.exportTemplate = $scope.exportTemplates[0];

        for (var i = 0; i < $scope.statusListAdmin.length; i++) {
            $scope.statusDict[$scope.statusListAdmin[i]] = $scope.statusListAdmin[i];
            if ($scope.statusListAdmin[i] === '确认发货(未到货)') {
                $scope.statusDict[$scope.statusListAdmin[i]] = '待入库';
            }
            else if ($scope.statusListAdmin[i] === '确认发货(已到货)' || $scope.statusListAdmin[i] === '未预报') {
                $scope.statusDict[$scope.statusListAdmin[i]] = '已入库';
            }
        }
        $scope.status = $scope.statusList[0];
        $scope.newPackageStatusList = ['待入库'];

        $scope.receiver = '';
        $scope.packageCode = '';
        $scope.tracking = '';
        $scope.addressIDs = [];
        $scope.senderIDs = [];
        $scope.selected = [];

        $scope.selectedPackages = [];
        $scope.options = {
            rowSelection: true,
            multiSelect: true,
        };

        $scope.init = function (callback) {
            $scope.getProductCategories();
            if ($scope.isAdmin) {
                $scope.newPackageStatusList = $scope.statusListAdmin;
                $scope.statusList = $scope.statusListAdmin;
            }
            if (callback) {
                callback();
            }
            $scope.searchPackages();
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
            $scope.selectedPackages = [];

            var req = {
                method: 'POST',
                url: '/Package/SearchPackages?nocache=' + new Date().getTime(),
                headers: {
                    'Cache-Control': 'no-cache'
                },
                data: { criteriaStr: JSON.stringify(model) }
            }
            $http(req).then(function (res) {
                $scope.packages = res.data;
                $scope.packagesOrder = '-LastUpdateTime';
            });
        }

        $scope.refreshPackages = function () {
            //$scope.startDate = new Date(new Date().getFullYear(), 0, 1);
            //$scope.endDate = new Date();
            //$scope.status = $scope.statusList[0];
            //$scope.receiver = '';
            //$scope.packageCode = '';
            //$scope.tracking = '';
            $scope.searchPackages();
        }

        $scope.getPackage = function (code, callback) {
            $http.get('/Package/GetPackage/' + code).then(function (res) {
                $scope.package = res.data;
                $scope.productsOrder = 'Number';
                if (typeof $scope.package.Address === 'undefined' || $scope.package.Address === null) { $scope.package.Address = {}; }
                if (typeof $scope.package.Sender === 'undefined' || $scope.package.Sender === null) { $scope.package.Sender = {}; }
                if (callback) {
                    callback();
                }
            });
        }

        $scope.shipPackage = function (code) {
            $scope.getPackage(code, function () {
                if (angular.equals($scope.package.Address, {})) {
                    alert('请选择或添加收件人');
                    return;
                }
                if (angular.equals($scope.package.Sender, {})) {
                    alert('请选择或添加发件人');
                    return;
                }
                if (confirm('确认发货以后将不能修改包裹信息、分箱、退货或者删除包裹，确认发货吗？')) {
                    if ($scope.package.Status === '待入库') {
                        $scope.package.SubStatus = '确认发货(未到货)';
                    }
                    else if ($scope.package.Status === '已入库') {
                        $scope.package.SubStatus = '确认发货(已到货)';
                    }
                    $scope.updatePackage();
                }
            });
        }
    
        $scope.trackPackage = function (code) {
            $scope.trackingPackage = code;
            $http.get('/Package/Tracking/' + code).then(function (res) {
                $scope.trackings = res.data;
                $('#trackingModal').modal('show');
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
            var status = '待入库';
            var subStatus = $scope.holdPackage ? '待入库' : '确认发货(未到货)';
            $scope.package = { Products: [{ Number: 1, Quantity: 1, Channel: 1 }], Status: status, SubStatus: subStatus, Sender: {}, Address: {}, WeightEst: 2 };
            $scope.showPackageModal();
        };

        $scope.showPackageModal = function () {
            $('#packageModal').modal({
                backdrop: 'static',
                keyboard: false
            });
            $('#packageModal').modal('show');
            $scope.getAddresses();
            $scope.getSenders();
        }

        $scope.getPackageCost = function () {
            $scope.showWeightAndCostLoading = true;
            $http.get('/Package/GetPackageCost/' + $scope.package.ID + '?weight=' + $scope.package.Weight).then(function (res) {
                $scope.showWeightAndCostLoading = false;
                $scope.package.Cost = res.data;
            });
        }

        //#region split package
        $scope.showSplitPackageModal = function () {
            $('#splitPackageModal').modal({
                backdrop: 'static',
                keyboard: false
            });
            $('#splitPackageModal').modal('show');
            $scope.newPackage = JSON.parse(JSON.stringify($scope.package));
            $scope.newPackage.ID = -1;
            $scope.newPackage.Products = [];
            $log.log($scope.package);
            $log.log($scope.newPackage);
        }

        $scope.splitProduct = function (index) {
            $scope.moveProduct($scope.package.Products, $scope.newPackage.Products, index);
        }

        $scope.cancelSplitProduct = function (index) {
            $scope.moveProduct($scope.newPackage.Products, $scope.package.Products, index);
        }

        $scope.moveProduct = function (products1, products2, index) {
            var product = products1[index];
            var existsInNewPackage = false;
            angular.forEach(products2, function (value) {
                if (value.ID === product.ID) {
                    existsInNewPackage = true;
                    value.Quantity++;
                }
            });
            if (!existsInNewPackage) {
                products2.push(JSON.parse(JSON.stringify(product)));
                products2[products2.length - 1].Quantity = 1;
            }

            if (product.Quantity === 1) {
                products1.splice(index, 1);
            }
            else {
                product.Quantity--;
            }
        }

        $scope.splitPackage = function () {
            $scope.updatePackage($scope.package);
            $scope.updatePackage($scope.newPackage);
        }
        //#endregion

        $scope.getProductCategories = function () {
            $http.get('/Package/GetProductCategories').then(function (res) {
                $scope.categories = res.data;
            });
        }

        $scope.addProduct = function () {
            $scope.package.Products.push({ Number: $scope.package.Products.length + 1, Quantity: 1, Channel: 1 });
        }

        $scope.removeProduct = function (index) {
            $scope.package.Products.splice(index, 1);
            angular.forEach($scope.package.Products, function (value, key) {
                value.Number = $scope.package.Products.indexOf(value) + 1;
            });
        }

        $scope.updatePackage = function (pkg) {
            if (typeof pkg === 'undefined') {
                // update package
                pkg = $scope.package;

                angular.forEach(pkg.Products, function (value, key) {
                    if (typeof value.Tracking === 'undefined' || value.Tracking === null) {
                        value.Tracking = pkg.Tracking;
                    }
                    value.OrderNumber = pkg.OrderNumber;
                    value.Notes = pkg.Notes;
                });
            }
            else {
                // update package after split
                pkg.Tracking = pkg.Products.map(function (product) { return product.Tracking; }).filter(function (value, index, self) { return self.indexOf(value) === index; }).join(';');
                pkg.OrderNumber = pkg.Products.map(function (product) { return product.OrderNumber; }).filter(function (value, index, self) { return self.indexOf(value) === index; }).join(';');
                pkg.Notes = pkg.Products.map(function (product) { return product.Notes; }).filter(function (value, index, self) { return self.indexOf(value) === index; }).join(';');
            }

            if (typeof pkg.Channel === 'undefined' || pkg.Channel === '') {
                alert('请选择渠道');
                return;
            }
            if (pkg.Channel === $scope.channelList[0] && !$appUtil.checkIDCard(pkg.Address.IDCard)) {
                //alert('身份证号码与标准格式不符，请检查');
            }
            //if (!$scope.local && (typeof pkg.Tracking === 'undefined' || pkg.Tracking === '')) {
            //    alert('请输入包裹追踪号再保存.')
            //    return;
            //}

            if (typeof pkg.ID === 'undefined' || pkg.ID === '') {
                $http.post('/Package/CheckTracking/' + pkg.Tracking).then(function (res) {
                    if (res.data === 'exist') { $appUtil.appAlert(null, '', '此包裹追踪号已存在，请输入其他追踪号'); return; }
                    $('#packageModal').modal('hide');
                    pkg.Status = $scope.statusDict[pkg.SubStatus];
                    $http.post('/Package/UpdatePackage', { package: pkg }).then(function (res) {
                        $scope.refreshPackages();
                    });
                });
            }
            else {
                $http.post('/Package/CheckPackageID/' + pkg.ID).then(function (res) {
                    if (res.data === 'exist' && pkg.ID !== pkg.OriginalID) { $appUtil.appAlert(null, '', '此单号已存在，请输入其他单号'); return; }
                    $('#packageModal').modal('hide');
                    pkg.Status = $scope.statusDict[pkg.SubStatus];
                    $http.post('/Package/UpdatePackage', { package: pkg }).then(function (res) {
                        $scope.refreshPackages();
                    });
                });
            }
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
            if (id === '' && targetAttr !== 'provinces') { return; }
            $http.get('/Address/GetDistricts/' + id + '?nocache=' + new Date().getTime()).then(function (res) {
                $scope[targetAttr] = [];
                angular.forEach(res.data, function (value, key) {
                    $scope[targetAttr].push({ ID: value.ID, Name: value.Name });
                    if (targetAttr === 'districts' && value.Name === '') {
                        $scope.package.Address.District = value.ID;
                    }
                });
            });
        }

        $scope.getAddresses = function (id) {
            if (typeof $scope.addresses === 'undefined' || $scope.addressIDs.indexOf($scope.package.Address.ID) === -1) {
                $http.post('/Address/GetAddresses/' + $scope.package.Address.ID).then(function (res) {
                    $scope.addresses = res.data;
                    if (typeof $scope.package.Address.ID === 'undefined' && $scope.addresses.length > 0) {
                        $scope.package.Address = {};
                    }
                    if (typeof id !== 'undefined') {
                        angular.forEach($scope.addresses, function (value) {
                            if (value.ID === id) {
                                $scope.package.Address = value;
                            }
                        });
                    }

                    $scope.updateAddressString();
                });
            }
        }

        $scope.updateAddress = function () {
            if (typeof $scope.package.Address.Phone === 'undefined' || $scope.package.Address.Phone === '') {
                alert('请输入电话号码'); return false;
            }
            if (typeof $scope.package.Address.District === 'undefined' || $scope.package.Address.District === '') {
                alert('请选择省份、城市和区县'); return false;
            }
            $('#addressModal').modal('hide');
            $http.post('/Address/UpdateAddress', { address: $scope.package.Address }).then(function (res) {
                $scope.getAddresses(res.data);
                $scope.updateAddressString();
            });
        }

        $scope.updateAddressString = function () {
            $scope.addressIDs = [];
            angular.forEach($scope.addresses, function (value, key) {
                value.addressString = value.Name + ' ' + value.ProvinceName + ' ' + value.CityName + ' ' + value.DistrictName + ' ' +
                    value.Street + ' ' + value.Phone;
                $scope.addressIDs.push(value.ID);
            });

            $scope.addresses.sort((a, b) => a.Name.localeCompare(b.Name, 'zh-CN'));

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

        $scope.addImage = function () {
            $fileReader.readAsDataUrl($scope.files[$scope.files.length - 1], $scope).then(function (result) {
                if (typeof $scope.package.Address.AddressIDCardImages=== 'undefined') {
                    $scope.package.Address.AddressIDCardImages = [];
                }
                $scope.package.Address.AddressIDCardImages.push({ Image: result });
            });
        };

        // End Address


        // Sender
        $scope.showSenderManager = function (newSender) {
            $('#senderModal').modal({
                backdrop: 'static',
                keyboard: false
            });
            $('#senderModal').modal('show');
            if (newSender) {
                $scope.package.Sender = {};
            }
        }

        $scope.getSenders = function () {
            if (typeof $scope.senders === 'undefined' || $scope.senderIDs.indexOf($scope.package.Sender.ID) === -1) {
                $http.post('/Address/GetSenders/' + $scope.package.Sender.ID).then(function (res) {
                    $scope.senders = res.data;
                    if (typeof $scope.package.Sender.ID === 'undefined') {
                        $scope.package.Sender = $scope.senders[0];
                    }
                    $scope.updateSenderString();
                });
            }
        }

        $scope.updateSender = function () {
            $http.post('/Address/UpdateSender', { sender: $scope.package.Sender }).then(function (res) {
                $scope.getSenders();
                $scope.updateSenderString();
            });
        }

        $scope.updateSenderString = function () {
            $scope.senderIDs = [];
            angular.forEach($scope.senders, function (value, key) {
                value.senderString = [value.Name, value.Address, value.Phone].join(' ');
                $scope.senderIDs.push(value.ID);
            });
        }

        $scope.deleteSender = function (id) {
            if (confirm('确认删除此发件人?')) {
                $http.get('/Address/DeleteSender/' + id).then(function (res) {
                    if (res.data !== '') { alert(res.data); }
                    for (i = $scope.senders.length - 1; i >= 0; i--) {
                        if ($scope.senders[i].ID === id) {
                            $scope.senders.splice(i, 1);
                            return;
                        }
                    }
                });
            }
        }

        // End Sender


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
                    packageCodes.push(value.ID);
                });
                $http.post('/Package/CombinePackages', { packageCodes: packageCodes }).then(function (res) {
                    $scope.refreshPackages();
                });
            }
        };

        $scope.export = function () {
            var packageIDs = $scope.selectedPackages.map(function (pkg) { return pkg.ID }).join('&ids=');
            var src = "/Package/ExportPackages?ids=" + packageIDs + '&template=' +$scope.exportTemplate;
            window.open(src, "", "", "");
        };

        $scope.print = function () {
            var packageIDs = $scope.selectedPackages.map(function (pkg) { return pkg.ID }).join('&ids=');
            var src = "/Package/PrintPackages?ids=" + packageIDs;
            window.open(src, "", "", "");
        };

        

        $scope.getPackageOverview = function () {
            $http.get('/Package/GetPackageOverview').then(function (res) {
                $scope.packageOverview = [];
                angular.forEach($scope.statusList, function (value, key) {
                    var count = 0;
                    var status = value;
                    angular.forEach(res.data, function (value, key) {
                        if (value.Status === status) {
                            count = value.Count;
                            return;
                        }
                    });
                    if (status !== '全部') {
                        $scope.packageOverview.push({ Status: status, Count: count });
                    }
                });
            });
        };

        $scope.uploadMFID = function () {
            var formData = new FormData();
            formData.append('file', $scope.childFileSelectScope.files[$scope.childFileSelectScope.files.length - 1]);
            $('body').addClass('loading');
            var request = {
                method: 'POST',
                url: '/Package/UploadMFID/',
                data: formData,
                headers: {
                    'Content-Type': undefined
                }
            };
            $http(request).then(function (res) {
                $('body').removeClass('loading');
                alert(res.data);
            }, function () {
                $('body').removeClass('loading');
                alert('匹配失败，联系你老公');
            });
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
    }]);