
// Write your JavaScript code.

$(document).ready(function () {
    if (userAuthorized) {
        $.ajax({
            url: '/Notifications/UserNotificationsCount',
            type: 'GET',
            dataType: 'json',
            contentType: 'application/json',
            cache: false,
            success: function (results) {
                console.log(results);
                console.log(results.length)
                if (results.length > 0) {
                    var donations = 0;
                    var requests = 0;
                    results.forEach(function (arrayItem) {
                        if (arrayItem.typeOfNoti === 0) {
                            donations = arrayItem.count;
                        } else if (arrayItem.typeOfNoti === 1) {
                            requests = arrayItem.count;
                        }
                    });
                    if (donations > 0) {
                        $('#donationNumber').text(donations);
                    }
                    else if (requests > 0) {
                        $('#requestNumber').text(requests);
                    }

                    //fetch notifications
                    $.ajax({
                        url: '/Notifications/UserNotifications',
                        type: 'GET',
                        dataType: 'json',
                        contentType: 'application/json',
                        cache: false,
                        success: function (notifications) {
                            console.log(notifications);
                            if (notifications.length > 0) {
                                notifications.forEach(function (arrayItem) {
                                    console.log(arrayItem.text);
                                    if (arrayItem.typeOfNoti === 0) {
                                        $('.donations').append(
                                            `<a class="dropdown-item" href="/Donations/GetDetailsFromMedicineId/${arrayItem.medicineId}">${arrayItem.text}</a>`
                                        );
                                    } else if (arrayItem.typeOfNoti === 1) {
                                        $('.requests').append(
                                            `<a class="dropdown-item" href="/Requests/GetDetailsFromMedicineId/${arrayItem.medicineId}">${arrayItem.text}</a>`
                                        );
                                    }
                                });
                            }
                        },
                        error: function () {
                            console.log("error occured in inner request");
                        }
                    });

                } else {
                    $('.dd').hide();
                }
                
            },
            error: function () {
                alert('Error occured');
            }
        });
        //
        $
    }
});

function deleteNotifications(type, username) {

    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    var notification = {
        __RequestVerificationToken: token,
        type: type,
        username: username,
    }
    $.ajax({
        url: "/Notifications/Delete",
        type: "POST",
        data: notification,
        success: function (result) {
            console.log(result);
            if (type === 0) {
                $('#ddh').hide();
            } else if (type === 1) {
                $('#ddr').hide();
            }
        },
        error: function (error) {
            alert(error);
        },
    });
}

function deleteRequestNotifications() {

}
/*
<a class="dropdown-item" href="#">buntent</a>
                    <div class="dropdown-divider"></div>
                    <a class="dropdown-item" href="#">Separated link</a>*/

function printDiv(divName) {
    var printContents = document.getElementById(divName).innerHTML;
    var originalContents = document.body.innerHTML;

    document.body.innerHTML = printContents;

    window.print();

    document.body.innerHTML = originalContents;
}



