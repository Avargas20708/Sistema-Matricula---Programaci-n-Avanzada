document.querySelectorAll('nav a').forEach(a=>{if(a.pathname===location.pathname)a.setAttribute('aria-current','page')});
