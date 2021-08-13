window.ShowToastr = (type, message) => {
    if (type == "success") {
        toastr.success(message, "Operation Successful.", { timeout: 10000 });
    }
    if (type == "error") {
        toastr.error(message, "Operation Failed.", { timeout: 10000 });
    }
}