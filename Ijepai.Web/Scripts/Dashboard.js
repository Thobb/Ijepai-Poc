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
                 $.ajax({
                     url: "/Dashboard/DeleteQCVM",
                     type: "POST",
                     data: "id="+id,
                    success: function () {
                                   //thisBtn.removeClass("glyphicon-play").addClass("glyphicon-stop").attr("title", "Click to stop the VM");
                     }
                   })
            }
        },
        QC_act: {
            policy: "Allow",
            title: "Capture VM",
            method: function (id) {
                $.ajax({
                    url: "/Dashboard/CaptureQCVM",
                    type: "POST",
                    data: "id=" + id,
                    success: function () {
                        //thisBtn.removeClass("glyphicon-play").addClass("glyphicon-stop").attr("title", "Click to stop the VM");
                    }
                })
            }

        }

    },
    subgrid: false
}
$(function () {
    Grid.Init("#QC-list","QuickCreateGrid")
})