document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('newsletter-form');
    const emailInput = document.getElementById('newsletter-email');
    const messageDiv = document.getElementById('subscription-message');

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        // Validate email
        if (!validateEmail(emailInput.value)) {
            showMessage('Please enter a valid email', 'error');
            return;
        }

        // Disable button during submission
        const submitBtn = form.querySelector('button');
        submitBtn.disabled = true;
        submitBtn.innerHTML = 'Submitting...';

        try {
            const response = await fetch('/api/newsletter/subscribe', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email: emailInput.value })
            });

            const data = await response.json();

            if (response.ok) {
                showMessage(data.message || 'Subscription successful!', 'success');
                form.reset();
            } else {
                showMessage(data.message || 'Subscription failed', 'error');
            }
        } catch (error) {
            showMessage('Network error. Please try again.', 'error');
        } finally {
            submitBtn.disabled = false;
            submitBtn.innerHTML = 'Subscribe';
        }
    });

    function validateEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    function showMessage(text, type) {
        messageDiv.textContent = text;
        messageDiv.className = `subscription-message ${type}`;
        setTimeout(() => messageDiv.style.opacity = '0', 5000);
    }
});



