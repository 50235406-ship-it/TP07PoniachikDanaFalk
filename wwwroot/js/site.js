function validoNombredeUsuario() {
    const nombreUsuario = document.getElementById('nombreUsuario');
    const mensaje = document.getElementById('mensajeNombreUsuario');
    
    if (!nombreUsuario || !mensaje) return;
    
    const valor = nombreUsuario.value.trim();
    const isValid = valor.length >= 3 && valor.length <= 20 ;
    
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
    
    if (document.getElementById('contraseña2')) {
        contraseñasIguales();
    }
}

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

function validoNombre() {
    const nombre = document.getElementById('nombre');
    const mensaje = document.getElementById('mensajeNombre');
    
    if (!nombre || !mensaje) return;
    
    const valor = nombre.value.trim();
    const isValid = valor.length >= 2 ;
    
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

function validoApellido() {
    const apellido = document.getElementById('apellido');
    const mensaje = document.getElementById('mensajeApellido');
    
    if (!apellido || !mensaje) return;
    
    const valor = apellido.value.trim();
    const isValid = valor.length >= 2 ;
    
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


const apiBase = '/Home';

const postList = document.getElementById('postList');
const loadMoreButton = document.getElementById('loadMoreButton');
const createForm = document.getElementById('crearPublicacionForm');
let desde = 0;

function escapeHtml(value = '') {
    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function formatDate(dateString) {
    const date = new Date(dateString);
    if (Number.isNaN(date.getTime())) {
        return 'Fecha no disponible';
    }

    return new Intl.DateTimeFormat('es-AR', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    }).format(date);
}

function renderComments(comentarios = []) {
    if (!comentarios || comentarios.length === 0) {
        return '<p class="empty-comments">Sin comentarios todavía.</p>';
    }

    return comentarios.map(comentario => `
        <div class="comment-item">
            <p class="comment-header"><strong>${escapeHtml(comentario.nombreUsuario || 'Usuario')}</strong> • ${formatDate(comentario.fechaComentario)}</p>
            <p class="comment-text">${escapeHtml(comentario.texto || '')}</p>
        </div>
    `).join('');
}

function renderPublicacion(publicacion) {
    return `
        <article class="post-item" data-id="${publicacion.id}">
            <div class="post-header">
                <div>
                    <h3>${escapeHtml(publicacion.titulo || '')}</h3>
                    <p class="post-meta">Por <strong>${escapeHtml(publicacion.nombreUsuario || 'Usuario')}</strong> • ${formatDate(publicacion.fechaPublicacion)}</p>
                </div>
            </div>
            <p class="post-body">${escapeHtml(publicacion.descripcion || '')}</p>

            <div class="post-actions">
                <button type="button" class="like-button ${publicacion.meGustaUsuario ? 'liked' : ''}" data-id="${publicacion.id}" data-liked="${publicacion.meGustaUsuario ? 'true' : 'false'}">
                    ${publicacion.meGustaUsuario ? 'Quitar me gusta' : 'Me gusta'}
                </button>
                <span class="likes-count">${Number(publicacion.cantidadLikes || 0)} me gusta</span>
            </div>

            <div class="comments-section">
                <h4>Comentarios</h4>
                <div class="comments-list">
                    ${renderComments(publicacion.comentarios || [])}
                </div>
                <form class="comment-form" data-id="${publicacion.id}">
                    <input type="text" name="texto" placeholder="Escribe un comentario..." maxlength="500" required />
                    <button type="submit">Comentar</button>
                </form>
            </div>
        </article>
    `;
}

function refrescarPublicaciones() {
    desde = 0;
    loadPublicaciones();
}

async function loadPublicaciones() {
    try {
        const response = await fetch(`${apiBase}/ObtenerPublicaciones?desde=${desde}&cantidad=10`);
        const data = await response.json();

        if (data.error) {
            postList.innerHTML = `<p class="empty-state">Error: ${escapeHtml(data.error)}</p>`;
            loadMoreButton.style.display = 'none';
            return;
        }

        const publicaciones = Array.isArray(data.publicaciones) ? data.publicaciones : [];

        if (desde === 0) {
            postList.innerHTML = '';
        }

        if (!publicaciones.length && desde === 0) {
            postList.innerHTML = '<p class="empty-state">Todavía no hay publicaciones. ¡Sé el primero en compartir algo!</p>';
            loadMoreButton.style.display = 'none';
            return;
        }

        if (publicaciones.length > 0) {
            postList.insertAdjacentHTML('beforeend', publicaciones.map(renderPublicacion).join(''));
            desde += publicaciones.length;
        }

        loadMoreButton.style.display = (data.hayMas && publicaciones.length > 0) ? 'inline-block' : 'none';
    } catch (error) {
        console.error('Error al cargar publicaciones:', error);
        postList.innerHTML = '<p class="empty-state">Ocurrió un error al cargar las publicaciones. Por favor, recarga la página.</p>';
        loadMoreButton.style.display = 'none';
    }
}

if (createForm) {
    createForm.addEventListener('submit', async (event) => {
        event.preventDefault();

        const formData = new FormData(createForm);
        try {
            const response = await fetch(`${apiBase}/GuardarPublicacion`, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                const errorText = await response.text();
                alert('No se pudo crear la publicación.');
                console.error('Error al crear publicación:', errorText);
                return;
            }

            createForm.reset();
            refrescarPublicaciones();
        } catch (error) {
            console.error('Error en formulario:', error);
            alert('Ocurrió un error al crear la publicación.');
        }
    });
}

document.addEventListener('DOMContentLoaded', () => {
    console.log('Página cargada, inicializando publicaciones...');
    if (postList) {
        loadPublicaciones();
    }
});

document.addEventListener('click', async (event) => {
    const button = event.target.closest('.like-button');
    if (!button) {
        return;
    }

    const idPublicacion = Number(button.dataset.id);

    try {
        const response = await fetch(`${apiBase}/ToggleLike`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ IdPublicacion: idPublicacion })
        });

        const resultado = await response.json();

        if (!resultado.ok) {
            alert(resultado.message || 'No se pudo actualizar el Me Gusta.');
            return;
        }

        const countElement = button.parentElement.querySelector('.likes-count');
        const liked = Boolean(resultado.liked);

        button.dataset.liked = liked ? 'true' : 'false';
        button.textContent = liked ? 'Quitar me gusta' : 'Me gusta';
        button.classList.toggle('liked', liked);
        countElement.textContent = `${resultado.cantidadLikes} me gusta`;
    } catch (error) {
        console.error('Error al actualizar Like:', error);
        alert('Ocurrió un error al actualizar el Me Gusta.');
    }
});

document.addEventListener('submit', async (event) => {
    const form = event.target.closest('.comment-form');
    if (!form) {
        return;
    }

    event.preventDefault();

    const idPublicacion = Number(form.dataset.id);
    const texto = form.querySelector('input[name="texto"]').value.trim();

    if (!texto) {
        return;
    }

    try {
        const response = await fetch(`${apiBase}/AgregarComentario`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ IdPublicacion: idPublicacion, Texto: texto })
        });

        const resultado = await response.json();

        if (!resultado.ok) {
            alert(resultado.message || 'No se pudo guardar el comentario.');
            return;
        }

        const commentsList = form.closest('.comments-section').querySelector('.comments-list');
        const emptyState = commentsList.querySelector('.empty-comments');
        if (emptyState) {
            emptyState.remove();
        }

        commentsList.insertAdjacentHTML('beforeend', `
            <div class="comment-item">
                <p class="comment-header"><strong>${escapeHtml(resultado.comentario.nombreUsuario || 'Usuario')}</strong> • ${formatDate(resultado.comentario.fechaComentario)}</p>
                <p class="comment-text">${escapeHtml(resultado.comentario.texto || '')}</p>
            </div>
        `);

        form.reset();
    } catch (error) {
        console.error('Error al guardar comentario:', error);
        alert('Ocurrió un error al guardar el comentario.');
    }
});

if (loadMoreButton) {
    loadMoreButton.addEventListener('click', loadPublicaciones);
}
