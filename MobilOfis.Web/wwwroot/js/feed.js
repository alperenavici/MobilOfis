(function () {
    const feedContainers = document.querySelectorAll('.feed-widget');
    if (!feedContainers.length) {
        return;
    }

    const escapeHtml = (value = '') =>
        value.replace(/[&<>"']/g, (char) => ({
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;',
        })[char] || char);

    const formatContent = (value = '') =>
        escapeHtml(value).replace(/\n/g, '<br>');

    const buildPostHtml = (post) => {
        const initials = (post.authorName || 'K')
            .split(' ')
            .map((part) => part.charAt(0))
            .join('')
            .substring(0, 2)
            .toUpperCase();

        const likeClass = post.isLiked ? 'text-danger' : 'text-muted';
        const likeIcon = post.isLiked ? 'bi-heart-fill' : 'bi-heart';

        return `
        <article class="card feed-post-card mb-3" data-post-id="${post.postId}">
            <div class="card-header border-0 pb-0 bg-transparent">
                <div class="d-flex align-items-center">
                    <div class="feed-post-card__avatar me-2">
                        ${post.authorAvatar
                ? `<img src="${post.authorAvatar}" alt="${escapeHtml(post.authorName)}" class="rounded-circle" width="40" height="40" />`
                : `<div class="avatar-placeholder rounded-circle bg-light text-primary d-flex align-items-center justify-content-center" style="width: 40px; height: 40px; font-weight: 600;">${escapeHtml(initials)}</div>`}
                    </div>
                    <div>
                        <h6 class="mb-0 fw-bold text-dark">${escapeHtml(post.authorName)}</h6>
                        <small class="text-muted" style="font-size: 0.8rem;">${escapeHtml(post.createdAtDisplay)}</small>
                    </div>
                    <div class="ms-auto">
                        <button class="btn btn-link text-muted p-0 text-decoration-none">
                            <i class="bi bi-three-dots"></i>
                        </button>
                    </div>
                </div>
            </div>
            <div class="card-body">
                ${post.content
                ? `<p class="card-text mb-2">${formatContent(post.content)}</p>`
                : ''}
                ${post.imageUrl
                ? `<div class="feed-post-card__image mt-2 rounded overflow-hidden">
                           <img src="${post.imageUrl}" alt="Post image" class="img-fluid w-100" style="object-fit: cover; max-height: 500px;" />
                       </div>`
                : ''}
            </div>
            <div class="card-footer bg-transparent border-top-0 pt-0">
                <div class="d-flex gap-2 mb-2">
                    <button class="btn btn-light btn-sm rounded-pill flex-grow-1 js-like-btn ${likeClass}" data-liked="${post.isLiked}">
                        <i class="bi ${likeIcon} me-1"></i> <span class="js-like-count">${post.likeCount}</span> Beğen
                    </button>
                    <button class="btn btn-light btn-sm rounded-pill text-muted flex-grow-1 js-comment-toggle-btn">
                        <i class="bi bi-chat me-1"></i> <span class="js-comment-count">${post.commentCount}</span> Yorum Yap
                    </button>
                </div>
                <div class="feed-post-card__comments d-none js-comments-section">
                    <div class="js-comments-list mb-2"></div>
                    <form class="js-comment-form d-flex gap-2">
                        <input type="text" class="form-control form-control-sm rounded-pill" placeholder="Yorum yaz..." name="content" required autocomplete="off">
                        <button type="submit" class="btn btn-primary btn-sm rounded-circle">
                            <i class="bi bi-send-fill"></i>
                        </button>
                    </form>
                </div>
            </div>
        </article>`;
    };

    const initFeed = (container) => {
        const listUrl = container.dataset.feedListUrl;
        const createUrl = container.dataset.feedCreateUrl;
        const pageSize = parseInt(container.dataset.feedPageSize || '5', 10);
        const form = container.querySelector('.js-feed-form');
        const listElement = container.querySelector('.js-feed-list');
        const loadMoreButton = container.querySelector('.js-feed-load-more');
        const statusElement = container.querySelector('.js-feed-status');
        const imageInput = container.querySelector('.js-feed-image-input');
        const selectedImageLabel = container.querySelector('.js-feed-selected-image');

        if (!listElement || !listUrl) {
            return;
        }

        let page = 1;
        let isLoading = false;
        let hasMore = true;

        const renderPosts = (posts, { reset = false } = {}) => {
            if (reset) {
                listElement.innerHTML = '';
            }

            if (!posts.length && listElement.children.length === 0) {
                listElement.innerHTML = '<p class="text-muted small mb-0">Henüz paylaşım yok.</p>';
                return;
            }

            posts.forEach((post) => {
                listElement.insertAdjacentHTML('beforeend', buildPostHtml(post));
            });
        };

        const toggleLoadMore = () => {
            if (!loadMoreButton) return;
            loadMoreButton.classList.toggle('d-none', !hasMore);
        };

        const setStatus = (message) => {
            if (statusElement) {
                statusElement.textContent = message || '';
            }
        };

        const loadPosts = async (reset = false) => {
            if (isLoading) {
                return;
            }

            if (reset) {
                page = 1;
            }

            isLoading = true;
            setStatus(reset ? 'Akış yükleniyor...' : '');

            try {
                const response = await fetch(`${listUrl}?page=${page}&pageSize=${pageSize}`, {
                    headers: { 'Accept': 'application/json' }
                });

                if (!response.ok) {
                    throw new Error('Akış yüklenemedi, lütfen daha sonra tekrar deneyin.');
                }

                const data = await response.json();
                renderPosts(data.posts || [], { reset });
                hasMore = !!data.hasMore;
                toggleLoadMore();

                if (hasMore) {
                    page += 1;
                }

                setStatus('');
            } catch (error) {
                setStatus(error.message || 'Beklenmeyen bir hata oluştu.');
            } finally {
                isLoading = false;
            }
        };

        const submitPost = async (event) => {
            event.preventDefault();
            if (!form || !createUrl) {
                return;
            }

            if (form.classList.contains('is-submitting')) {
                return;
            }

            const formData = new FormData(form);
            const hasContent = (formData.get('Content') || '').toString().trim().length > 0;
            const hasImage = imageInput?.files?.length > 0;

            if (!hasContent && !hasImage) {
                setStatus('Paylaşım metni veya görsel yükleyin.');
                return;
            }

            form.classList.add('is-submitting');
            setStatus('Paylaşım gönderiliyor...');

            try {
                const response = await fetch(createUrl, {
                    method: 'POST',
                    body: formData
                });

                const data = await response.json();

                if (!response.ok || !data.success) {
                    throw new Error(data.errors?.join(', ') || data.message || 'Paylaşım sırasında hata oluştu.');
                }

                form.reset();
                if (selectedImageLabel) {
                    selectedImageLabel.textContent = '';
                }

                setStatus('Paylaşım yayınlandı.');
                await loadPosts(true);
            } catch (error) {
                setStatus(error.message || 'Paylaşım gönderilemedi.');
            } finally {
                form.classList.remove('is-submitting');
            }
        };

        const handleImageChange = () => {
            if (!selectedImageLabel) return;
            if (imageInput?.files?.length) {
                selectedImageLabel.textContent = imageInput.files[0].name;
            } else {
                selectedImageLabel.textContent = '';
            }
        };

        // Social Actions
        const handleLike = async (btn) => {
            const card = btn.closest('.feed-post-card');
            const postId = card.dataset.postId;
            const isLiked = btn.dataset.liked === 'true';
            const url = isLiked ? '/Post/Unlike' : '/Post/Like';

            try {
                const response = await fetch(`${url}?postId=${postId}`, { method: 'POST' });
                const data = await response.json();

                if (data.success) {
                    btn.dataset.liked = (!isLiked).toString();
                    btn.classList.toggle('text-danger');
                    btn.classList.toggle('text-muted');
                    const icon = btn.querySelector('i');
                    icon.classList.toggle('bi-heart');
                    icon.classList.toggle('bi-heart-fill');
                    btn.querySelector('.js-like-count').textContent = data.count;
                }
            } catch (error) {
                console.error('Like error:', error);
            }
        };

        const loadComments = async (card) => {
            const postId = card.dataset.postId;
            const list = card.querySelector('.js-comments-list');

            try {
                const response = await fetch(`/Post/GetComments?postId=${postId}`);
                const data = await response.json();

                if (data.success) {
                    list.innerHTML = data.comments.map(c => `
                        <div class="d-flex gap-2 mb-2">
                            <img src="${c.authorAvatar || ''}" class="rounded-circle" width="24" height="24" style="object-fit: cover;">
                            <div class="bg-light rounded px-3 py-2 flex-grow-1">
                                <div class="d-flex justify-content-between align-items-center mb-1">
                                    <span class="fw-bold small">${escapeHtml(c.authorName)}</span>
                                    <small class="text-muted" style="font-size: 0.7rem;">${c.createdAtDisplay}</small>
                                </div>
                                <p class="mb-0 small">${formatContent(c.content)}</p>
                            </div>
                        </div>
                    `).join('');
                }
            } catch (error) {
                console.error('Load comments error:', error);
            }
        };

        const handleCommentToggle = (btn) => {
            const card = btn.closest('.feed-post-card');
            const section = card.querySelector('.js-comments-section');
            section.classList.toggle('d-none');

            if (!section.classList.contains('d-none')) {
                loadComments(card);
            }
        };

        const handleCommentSubmit = async (form) => {
            const card = form.closest('.feed-post-card');
            const postId = card.dataset.postId;
            const input = form.querySelector('input[name="content"]');
            const content = input.value.trim();

            if (!content) return;

            try {
                const formData = new FormData();
                formData.append('postId', postId);
                formData.append('content', content);

                const response = await fetch('/Post/Comment', {
                    method: 'POST',
                    body: formData
                });

                const data = await response.json();

                if (data.success) {
                    input.value = '';
                    card.querySelector('.js-comment-count').textContent = data.count;
                    loadComments(card); // Reload to show new comment
                }
            } catch (error) {
                console.error('Comment error:', error);
            }
        };

        // Event Delegation
        listElement?.addEventListener('click', (e) => {
            const likeBtn = e.target.closest('.js-like-btn');
            if (likeBtn) {
                handleLike(likeBtn);
                return;
            }

            const commentBtn = e.target.closest('.js-comment-toggle-btn');
            if (commentBtn) {
                handleCommentToggle(commentBtn);
                return;
            }
        });

        listElement?.addEventListener('submit', (e) => {
            const commentForm = e.target.closest('.js-comment-form');
            if (commentForm) {
                e.preventDefault();
                handleCommentSubmit(commentForm);
            }
        });

        form?.addEventListener('submit', submitPost);
        imageInput?.addEventListener('change', handleImageChange);
        loadMoreButton?.addEventListener('click', () => loadPosts(false));

        loadPosts(true);
    };

    feedContainers.forEach(initFeed);
})();

