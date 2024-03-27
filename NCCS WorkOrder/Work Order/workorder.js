

$(document).ready(function () {

    // xxxxxx
    $('#xxxxxx').select2({
        theme: 'classic',
        placeholder: 'Select here.....',
        allowClear: false,
    });

    // requisition no
    $('#ddScRequisitionNo').select2({
        theme: 'classic',
        placeholder: 'Select here.....',
        allowClear: false,
    });

    // mode of payment
    $('#ModeOfPayment').select2({
        theme: 'classic',
        placeholder: 'Select here.....',
        allowClear: false,
    });

    // tds section
    $('#TDSSection').select2({
        theme: 'classic',
        placeholder: 'Select here.....',
        allowClear: false,
    });





    // Reinitialize Select2 after UpdatePanel partial postback
    var prm = Sys.WebForms.PageRequestManager.getInstance();

    // Reinitialize Select2 for all dropdowns
    prm.add_endRequest(function () {

        setTimeout(function () {

            // requisition no
            $('#ddScRequisitionNo').select2({
                theme: 'classic',
                placeholder: 'Select here.....',
                allowClear: false,
            });

            // mode of payment
            $('#ModeOfPayment').select2({
                theme: 'classic',
                placeholder: 'Select here.....',
                allowClear: false,
            });

            // tds section
            $('#TDSSection').select2({
                theme: 'classic',
                placeholder: 'Select here.....',
                allowClear: false,
            });

        }, 0);
    });

});