function validateContactFormSubmit() {
    let x = document.forms["contactForm"]["email"].value;
    if (x == "") {
        alert("Vous devez entrer votre courriel.");
        return false;
    }
    let regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!regex.test(x)) {
        alert("Entrez un courriel valide.");
        return false;
    }

    return true;
}