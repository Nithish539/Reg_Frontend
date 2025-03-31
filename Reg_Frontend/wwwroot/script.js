document.addEventListener("DOMContentLoaded", function () {
    loadStates();

    document.getElementById("registrationForm").addEventListener("submit", function (event) {
        event.preventDefault();
        registerUser();
    });
});

async function loadStates() {
    try {
        let response = await fetch("https://yourapi.com/api/states");  // Replace with your API URL
        let states = await response.json();
        let stateDropdown = document
