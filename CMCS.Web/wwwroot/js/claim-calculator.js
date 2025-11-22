// PSEUDOCODE:
// - Use jQuery's ready shortcut $(function() { ... }) to avoid the deprecated $(document).ready(handler) overload.
// - Query elements: #HoursWorked, #HourlyRate, #totalAmount, #hoursError, #rateError.
// - calculateTotal(): parse numeric values (default 0), compute total, animate update of total display, call validators.
// - validateHours(hours): mark input invalid if <= 0 or > 744, otherwise mark valid and clear error.
// - validateRate(rate): mark input invalid if <= 0 or > 10000, otherwise mark valid and clear error.
// - Attach 'input' event handlers for both inputs to re-calculate on change.
// - Run initial calculation on load.

// Automated claim calculation
$(function () {
    const hoursInput = $('#HoursWorked');
    const rateInput = $('#HourlyRate');
    const totalDisplay = $('#totalAmount');

    // Auto-calculate on input change
    function calculateTotal() {
        const hours = parseFloat(hoursInput.val()) || 0;
        const rate = parseFloat(rateInput.val()) || 0;
        const total = hours * rate;

        // Update display with animation
        totalDisplay.fadeOut(200, function () {
            $(this).text('R ' + total.toFixed(2)).fadeIn(200);
        });

        // Validation feedback
        validateHours(hours);
        validateRate(rate);
    }

    function validateHours(hours) {
        if (hours > 744) {
            hoursInput.addClass('is-invalid').removeClass('is-valid');
            $('#hoursError').text('Hours cannot exceed 744 per month');
        } else if (hours <= 0) {
            hoursInput.addClass('is-invalid').removeClass('is-valid');
            $('#hoursError').text('Hours must be greater than 0');
        } else {
            hoursInput.removeClass('is-invalid').addClass('is-valid');
            $('#hoursError').text('');
        }
    }

    function validateRate(rate) {
        if (rate > 10000) {
            rateInput.addClass('is-invalid').removeClass('is-valid');
            $('#rateError').text('Rate cannot exceed R10,000');
        } else if (rate <= 0) {
            rateInput.addClass('is-invalid').removeClass('is-valid');
            $('#rateError').text('Rate must be greater than 0');
        } else {
            rateInput.removeClass('is-invalid').addClass('is-valid');
            $('#rateError').text('');
        }
    }

    // Attach event listeners
    hoursInput.on('input', calculateTotal);
    rateInput.on('input', calculateTotal);

    // Initial calculation
    calculateTotal();
});