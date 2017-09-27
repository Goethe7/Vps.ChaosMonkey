window.datarefreshId = 0;

showWait();

$(document).ready(function () {

    var interval = $.cookie("chaos-monitor-interval");
    var autorefresh = $.cookie("chaos-monitor-autorefresh");

    $(document).ajaxStop(function () {
        hideWait();
    });

    $(document).ajaxStart(function () {
        showWait();
    });

    $('#btnStart').click(function (e) {
        jQuery.event.trigger("ajaxStart");
        e.preventDefault();
    });

    $('#btnStop').click(function (e) {
        jQuery.event.trigger("ajaxStop");
        e.preventDefault();
    });

    if (isNaN(interval) || interval < 10 || interval > 300) {
        interval = 10;
        $.cookie("chaos-monitor-interval", interval);
    }

    $("#autorefresh-toggle").bootstrapToggle(autorefresh);
    $("#intervalSpinner").val(interval);

    setInterval("refreshPage()", interval * 1000);

    hideWait();
});

function refreshPage() {
    if (autorefreshEnabled()) {
        location.reload(true);
    }
}

function showWait() {
    $("body").css('cursor', 'wait');
    $("#loader").removeClass("hide");
    $("#loader").addClass("show");
    $("#waitertext").html("Please wait ...");
}

function hideWait() {
    window.datarefresh = setTimeout(function () {
        $("#loader").removeClass("show");
        $("#loader").addClass("hide");
    }, 2000);

    $("body").css('cursor', 'default');
}

$("#intervalSpinner").change(function () {
    var interval = $("#intervalSpinner").val();

    if (isNaN(interval) || interval < 10 || interval > 300) {
        interval = 60;
    }

    $.cookie("chaos-monitor-interval", interval);

    $("#intervalSpinner").val(interval);

    setInterval("refreshPage()", interval * 1000);
});


$("#autorefresh-toggle")
    .change(function () {
        $.cookie("chaos-monitor-autorefresh", $(this).prop("checked") ? "on" : "off");

        if ($(this).prop("checked")) {
            disableMonitorTable(true);
            disableIntervalBlock(false);
        } else {
            disableMonitorTable(false);
            disableIntervalBlock(true);
        }
    });

function disableMonitorTable(status) {
    $("#monitorTable").find("input,button").attr("disabled", status);
}

function disableIntervalBlock(status) {
    $("#disableIntervalBlock").find("input,button").attr("disabled", status);
}

function autorefreshEnabled() {
    return $("#autorefresh-toggle").prop("checked");
}

$(".service-action").submit(function (event) {

    var action = $(this).find("input[name='action']").val();
    var service = $(this).find("input[name='service']").val();

    var currentForm = this;

    event.preventDefault();

    bootbox.confirm({
        title: action + " Service Action Request",
        message: "Do you want to " + action + " the " + service + " service?",
        buttons: {
            cancel:  { label: '<i class="fa fa-times"></i> Cancel' },
            confirm: { label: '<i class="fa fa-check"></i> Confirm' }
        },
        callback: function (result) {
            
            if (result) {
                currentForm.submit();
            } else {
                // TODO
            }
        }
    });

});
