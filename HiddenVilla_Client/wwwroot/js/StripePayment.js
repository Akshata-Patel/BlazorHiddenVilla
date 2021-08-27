redirectToCheckout = function (sessionId) {
    var stripe = Stripe('pk_test_51JO3AcSAzvfjHwWDqdXQlEyPSN4hWQyDUzT69FFYwsdMRvqgDWD1MliiiUcQYY5kOp5tmQuTdZbY6Ag1Jdoa3SAZ00h1ayCPMx');
    stripe.redirectToCheckout({
        sessionId: sessionId
    });
}