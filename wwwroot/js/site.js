// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Validación de Nombre de Usuario
function validoNombredeUsuario() {
    const nombreUsuario = document.getElementById('nombreUsuario');
    const mensaje = document.getElementById('mensajeNombreUsuario');
    
    if (!nombreUsuario || !mensaje) return;
    
    const valor = nombreUsuario.value.trim();
    const isValid = valor.length >= 3 && valor.length <= 20 && /^[a-zA-Z0-9_-]+$/.test(valor);
    
    if (valor === '') {
        mensaje.classList.remove('visible');
        nombreUsuario.parentElement.classList.remove('error');
    } else if (!isValid) {
        mensaje.textContent = 'El nombre debe tener 3-20 caracteres (letras, números, _ y -)';
        mensaje.classList.add('visible');
        nombreUsuario.parentElement.classList.add('error');
    } else {
        mensaje.classList.remove('visible');
        nombreUsuario.parentElement.classList.remove('error');
    }
}

// Validación de Contraseña
function validoContraseña() {
    const contraseña = document.getElementById('contraseña');
    const mensaje = document.getElementById('mensajeContraseña');
    
    if (!contraseña || !mensaje) return;
    
    const valor = contraseña.value;
    const isValid = valor.length >= 8;
    
    if (valor === '') {
        mensaje.classList.remove('visible');
        contraseña.parentElement.classList.remove('error');
    } else if (!isValid) {
        mensaje.textContent = 'La contraseña debe tener al menos 8 caracteres';
        mensaje.classList.add('visible');
        contraseña.parentElement.classList.add('error');
    } else {
        mensaje.classList.remove('visible');
        contraseña.parentElement.classList.remove('error');
    }
    
    // Validar que las contraseñas coincidan si existe el segundo campo
    if (document.getElementById('contraseña2')) {
        contraseñasIguales();
    }
}

// Validación de Confirmación de Contraseña
function contraseñasIguales() {
    const contraseña1 = document.getElementById('contraseña');
    const contraseña2 = document.getElementById('contraseña2');
    const mensaje = document.getElementById('mensajeContraseña2');
    
    if (!contraseña1 || !contraseña2 || !mensaje) return;
    
    const valor1 = contraseña1.value;
    const valor2 = contraseña2.value;
    
    if (valor2 === '') {
        mensaje.classList.remove('visible');
        contraseña2.parentElement.classList.remove('error');
    } else if (valor1 !== valor2) {
        mensaje.textContent = 'Las contraseñas no coinciden';
        mensaje.classList.add('visible');
        contraseña2.parentElement.classList.add('error');
    } else {
        mensaje.classList.remove('visible');
        contraseña2.parentElement.classList.remove('error');
    }
}

// Validación de Nombre
function validoNombre() {
    const nombre = document.getElementById('nombre');
    const mensaje = document.getElementById('mensajeNombre');
    
    if (!nombre || !mensaje) return;
    
    const valor = nombre.value.trim();
    const isValid = valor.length >= 2 && /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/.test(valor);
    
    if (valor === '') {
        mensaje.classList.remove('visible');
        nombre.parentElement.classList.remove('error');
    } else if (!isValid) {
        mensaje.textContent = 'El nombre debe tener al menos 2 caracteres y contener solo letras';
        mensaje.classList.add('visible');
        nombre.parentElement.classList.add('error');
    } else {
        mensaje.classList.remove('visible');
        nombre.parentElement.classList.remove('error');
    }
}

// Validación de Apellido
function validoApellido() {
    const apellido = document.getElementById('apellido');
    const mensaje = document.getElementById('mensajeApellido');
    
    if (!apellido || !mensaje) return;
    
    const valor = apellido.value.trim();
    const isValid = valor.length >= 2 && /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/.test(valor);
    
    if (valor === '') {
        mensaje.classList.remove('visible');
        apellido.parentElement.classList.remove('error');
    } else if (!isValid) {
        mensaje.textContent = 'El apellido debe tener al menos 2 caracteres y contener solo letras';
        mensaje.classList.add('visible');
        apellido.parentElement.classList.add('error');
    } else {
        mensaje.classList.remove('visible');
        apellido.parentElement.classList.remove('error');
    }
}
