function addNavbarBurger () {
  // Get all "navbar-burger" elements
  var $navbarBurgers = Array.prototype.slice.call(document.querySelectorAll('.navbar-burger'), 0);

  // Check if there are any navbar burgers
  if ($navbarBurgers.length > 0) {

    // Add a click event on each of them
    $navbarBurgers.forEach(function ($el) {
      $el.addEventListener('click', function () {

        // Get the target from the "data-target" attribute
        var target = $el.dataset.target;
        var $target = document.getElementById(target);

        // Toggle the class on both the "navbar-burger" and the "navbar-menu"
        $el.classList.toggle('is-active');
        $target.classList.toggle('is-active');

      });
    });
  }
}

function addDeleteButtons () {
  var $deleteButtons = Array.prototype.slice.call(document.querySelectorAll('.is-delete'), 0);
  if ($deleteButtons.length > 0) {
    $deleteButtons.forEach(function($el) {
      $el.addEventListener('click', function () {
        var target = $el.dataset.href;
        var xhr = new XMLHttpRequest();
        xhr.open("DELETE", target, true);
        xhr.setRequestHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        xhr.onload = function () {
          window.location.reload(false);
        }
        xhr.send(null);
      });
    });
  }
}

document.addEventListener('DOMContentLoaded', function () {
  addNavbarBurger();
  addDeleteButtons();
});