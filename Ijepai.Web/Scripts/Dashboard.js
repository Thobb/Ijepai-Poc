Grids["QuickCreateGrid"] = {
    classPrefix: "QC",
    url: "/Dashboard/GetQC",
    id: "ID",
    columns: {
        Name: { title: "VM Name" },
        Machine_Size: { title: "Machine Size" },
        OSLabel: { title: "OS", style: "width: 185px" },
        RecepientEmail: { title: "Email Address", style: "width: 185px" },
        Status: { title: "Status", style: "width: 100px" },
        QCActions: { title: "Actions"}
    },
    actions: {
        QC_act: {
            policy: "Allow",
            title: "Delete VM",
            method: function (id) {
                Dashboard.showDeleteQCVMForm(id);
            }
        },
        QC_Capt: {
            policy: "Allow",
            title: "Capture VM",
            method: function (id) {
                Dashboard.showCaptureQCVMForm(id);
            }
        }
    },
    subgrid: false
}

var Dashboard = {
    deleteQCVM: function(id){
        $.ajax({
            url: "/Dashboard/DeleteQCVM",
            type: "POST",
            data: "id="+id,
            success: function () {
                //thisBtn.removeClass("glyphicon-play").addClass("glyphicon-stop").attr("title", "Click to stop the VM");
            }
        })
    },
    deleteQCVMSuccess: function(data){

    },
    showDeleteQCVMForm: function (id) {
        $("#overlay").fadeIn(function () {
            $("#delete_QCVM_id").val(id);
            $("#QC-VM-delete").fadeIn(200);
        });
    },
    hideDeleteQCVMForm: function () {
        $("#QC-VM-delete").fadeOut(function () {
            $("#overlay").fadeOut(200);
            $("#QC-VM-delete-form").trigger("reset");
        })
    },
    showQCForm: function() {
        $("#overlay").fadeIn(function () {
            $("#QC-form-content").fadeIn(200);
        });
    },
    hideQCForm: function() {
        $("#QC-form-content").fadeOut(function () {
            $("#overlay").fadeOut(200);
            $("#qc-create-form").trigger("reset");
        });
    },
    showCaptureQCVMForm: function (id) {
        $("#overlay").fadeIn(function () {
            $("#QCVM_capture_id").val(id);
            $("#QC-VM-capture").fadeIn(200);
        });
    },
    hideCaptureQCVMForm: function (id) {
        $("#QC-VM-capture").fadeOut(function () {
            $("#overlay").fadeOut(200);
            $("#QC-VM-capture-form").trigger("reset");
        })
    },
    captureQCVM: function(id) {
        $.ajax({
            url: "/Dashboard/CaptureQCVM",
            type: "POST",
            data: "id=" + id,
            success: function () {
                //thisBtn.removeClass("glyphicon-play").addClass("glyphicon-stop").attr("title", "Click to stop the VM");
            }
        })
        Dashboard.hideQCForm();
    },
    captureQCVMSuccess: function (data) {

    }
}

App.UpdateVMStatus = function (data) {
    Dashboard.hideQCForm();
    var statusTimerHandle = setInterval(function () {
        $.ajax({
            url: "/Dashboard/GetVMStatus",
            type: "POST",
            data: "ServiceName=" + data.ServiceName + "&VMName=" + data.VMName,
            success: function (data, status, xhr) {
                if (data.Status == "0") {
                    alert("<b>Machine state : <b>" + data.InstanceStatus + "<br><b>Power state : </b>" + data.PowerState);
                } else {
                    alert("Some error occured")
                }
                if ((data.InstanceStatus == "ReadyRole") && (data.PowerState == "Started")) {
                    clearInterval(statusTimerHandle)
                }
            }
        })
    }, 10000);
}
$(function () {
    Grid.Init("#QC-list", "QuickCreateGrid");
    $("#QC-form-content-close").click(function () {
        Dashboard.hideQCForm();
    });
    $("#open-QC-form").click(function (e) {
        e.preventDefault();
        Dashboard.showQCForm();
        return false;
    });
    $("#QC-VM-capture-close").click(function () {
        Dashboard.hideCaptureQCVMForm();
    })
    $("#QC-VM-capture-cancel").click(function () {
        Dashboard.hideCaptureQCVMForm();
    });
    $("#QC-VM-delete-close").click(function () {
        Dashboard.hideDeleteQCVMForm();
    });
    $("#cancel-QCVM-deletion").click(function () {
        Dashboard.hideDeleteQCVMForm();
    });
})