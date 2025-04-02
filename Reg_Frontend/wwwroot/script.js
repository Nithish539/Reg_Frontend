document.addEventListener("DOMContentLoaded", function () {
    loadStates();

    document.getElementById("registrationForm").addEventListener("submit", function (event) {
        event.preventDefault();
        registerUser();
    });
});

async function loadStates() {
    try {
        let response = await fetch("https://yourapi.com/api/states");
        let states = await response.json();
        let stateDropdown = document
