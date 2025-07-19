document.addEventListener('DOMContentLoaded', function() {
    initializePlayersPage();
});

// Initialize comprehensive players page functionality including search, sorting, and interactive features
function initializePlayersPage() {
    const searchInput = document.querySelector('.search-input input');
    const clearButton = document.querySelector('.clear-search');
    const table = document.querySelector('.table tbody');
    const tableHeaders = document.querySelectorAll('.table th.sortable');
    
    if (!searchInput || !clearButton || !table) {
        console.warn('Players page elements not found');
        return;
    }
    
    initializeSearch(searchInput, clearButton, table);
    initializeTableSorting(tableHeaders);
    initializeKeyboardShortcuts(searchInput);
    initializeProgressiveLoading();
    initializeTooltips();
    initializeTableInteractions();
    initializePlayerStats();
}

// Real-time search functionality with debouncing and highlighting
function initializeSearch(input, clearButton, table) {
    const originalRows = Array.from(table.querySelectorAll('tr'));
    let searchTimeout;
    
    input.addEventListener('input', function() {
        const searchTerm = this.value.toLowerCase().trim();
        
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            performSearch(searchTerm, originalRows, table);
        }, 150);
        
        if (searchTerm) {
            clearButton.classList.add('visible');
        } else {
            clearButton.classList.remove('visible');
        }
    });
    
    clearButton.addEventListener('click', function() {
        input.value = '';
        clearButton.classList.remove('visible');
        performSearch('', originalRows, table);
        input.focus();
    });
    
    input.addEventListener('focus', function() {
        this.parentElement.classList.add('focused');
    });
    
    input.addEventListener('blur', function() {
        this.parentElement.classList.remove('focused');
    });
}

// Perform search with highlighting and smooth animations
function performSearch(searchTerm, rows, table) {
    let visibleCount = 0;
    
    rows.forEach((row, index) => {
        const playerName = row.querySelector('.player-details h6');
        const playerEmail = row.querySelector('.player-details small');
        
        if (!playerName || !playerEmail) return;
        
        const nameMatch = playerName.textContent.toLowerCase().includes(searchTerm);
        const emailMatch = playerEmail.textContent.toLowerCase().includes(searchTerm);
        const isVisible = nameMatch || emailMatch || searchTerm === '';
        
        if (isVisible) {
            row.style.display = '';
            row.style.animationDelay = `${index * 0.05}s`;
            row.classList.add('search-result');
            visibleCount++;
        } else {
            row.style.display = 'none';
            row.classList.remove('search-result');
        }
    });
    
    if (visibleCount === 0 && searchTerm) {
        showNoResults(table);
    } else {
        hideNoResults(table);
    }
    
    updateResultsCount(visibleCount, rows.length);
}

// Advanced table sorting with multiple data type support
function initializeTableSorting(headers) {
    let currentSort = { column: null, direction: null };
    
    headers.forEach((header, index) => {
        header.addEventListener('click', function() {
            const table = this.closest('table');
            const tbody = table.querySelector('tbody');
            const rows = Array.from(tbody.querySelectorAll('tr:not(.no-results-row)'));
            
            const isCurrentColumn = currentSort.column === index;
            const newDirection = isCurrentColumn && currentSort.direction === 'asc' ? 'desc' : 'asc';
            
            headers.forEach(h => h.classList.remove('sorted-asc', 'sorted-desc'));
            this.classList.add(`sorted-${newDirection}`);
            
            currentSort = { column: index, direction: newDirection };
            
            rows.sort((a, b) => {
                const aCell = a.children[index];
                const bCell = b.children[index];
                
                if (!aCell || !bCell) return 0;
                
                const aValue = extractSortValue(aCell);
                const bValue = extractSortValue(bCell);
                
                let comparison = 0;
                
                if (typeof aValue === 'number' && typeof bValue === 'number') {
                    comparison = aValue - bValue;
                } else {
                    comparison = String(aValue).localeCompare(String(bValue));
                }
                
                return newDirection === 'asc' ? comparison : -comparison;
            });
            
            rows.forEach((row, index) => {
                row.style.animationDelay = `${index * 0.02}s`;
                tbody.appendChild(row);
            });
            
            this.classList.add('sorting');
            setTimeout(() => {
                this.classList.remove('sorting');
            }, 600);
        });
    });
}

// Extract appropriate sort value from table cell based on content type
function extractSortValue(cell) {
    if (cell.querySelector('.win-rate-text')) {
        return parseFloat(cell.querySelector('.win-rate-text').textContent.replace('%', ''));
    }
    
    if (cell.querySelector('.player-details h6')) {
        return cell.querySelector('.player-details h6').textContent.trim();
    }
    
    if (cell.querySelector('.status-badge')) {
        const badge = cell.querySelector('.status-badge');
        const statusOrder = { 'online': 3, 'away': 2, 'offline': 1 };
        return statusOrder[badge.textContent.toLowerCase()] || 0;
    }
    
    const numericValue = parseFloat(cell.textContent.replace(/[^\d.-]/g, ''));
    if (!isNaN(numericValue)) {
        return numericValue;
    }
    
    return cell.textContent.trim();
}

// Keyboard shortcuts for power users
function initializeKeyboardShortcuts(searchInput) {
    document.addEventListener('keydown', function(e) {
        if (e.ctrlKey && e.key === 'f') {
            e.preventDefault();
            searchInput.focus();
            searchInput.select();
        }
        
        if (e.key === 'Escape') {
            if (document.activeElement === searchInput) {
                searchInput.blur();
                if (searchInput.value) {
                    searchInput.value = '';
                    searchInput.dispatchEvent(new Event('input'));
                }
            }
        }
        
        if (e.key === 'ArrowDown' && document.activeElement === searchInput) {
            e.preventDefault();
            const firstRow = document.querySelector('.table tbody tr:not([style*="display: none"])');
            if (firstRow) {
                firstRow.focus();
            }
        }
    });
}

// Progressive loading animation for better perceived performance
function initializeProgressiveLoading() {
    const tableRows = document.querySelectorAll('.table tbody tr');
    
    tableRows.forEach((row, index) => {
        row.style.opacity = '0';
        row.style.transform = 'translateY(20px)';
        
        setTimeout(() => {
            row.style.transition = 'all 0.4s ease-out';
            row.style.opacity = '1';
            row.style.transform = 'translateY(0)';
        }, index * 50);
    });
}

// Enhanced tooltips with dynamic positioning
function initializeTooltips() {
    const tooltipElements = document.querySelectorAll('[data-tooltip]');
    
    tooltipElements.forEach(element => {
        let tooltip = null;
        let tooltipTimeout;
        
        element.addEventListener('mouseenter', function() {
            const text = this.getAttribute('data-tooltip');
            if (!text) return;
            
            tooltipTimeout = setTimeout(() => {
                tooltip = createTooltip(text);
                document.body.appendChild(tooltip);
                positionTooltip(tooltip, this);
                
                setTimeout(() => {
                    if (tooltip) tooltip.classList.add('show');
                }, 10);
            }, 300);
        });
        
        element.addEventListener('mouseleave', function() {
            clearTimeout(tooltipTimeout);
            if (tooltip) {
                tooltip.classList.remove('show');
                setTimeout(() => {
                    if (tooltip && tooltip.parentNode) {
                        tooltip.parentNode.removeChild(tooltip);
                    }
                }, 200);
            }
        });
    });
}

// Create and style tooltip element
function createTooltip(text) {
    const tooltip = document.createElement('div');
    tooltip.className = 'custom-tooltip';
    tooltip.textContent = text;
    
    tooltip.style.cssText = `
        position: absolute;
        background: rgba(0, 0, 0, 0.9);
        color: white;
        padding: 0.75rem 1rem;
        border-radius: 8px;
        font-size: 0.9rem;
        z-index: 1000;
        pointer-events: none;
        opacity: 0;
        transition: opacity 0.3s ease;
        backdrop-filter: blur(10px);
        white-space: nowrap;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
    `;
    
    return tooltip;
}

// Position tooltip with smart boundary detection
function positionTooltip(tooltip, element) {
    const rect = element.getBoundingClientRect();
    const tooltipRect = tooltip.getBoundingClientRect();
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    
    let left = rect.left + rect.width / 2 - tooltipRect.width / 2;
    let top = rect.top - tooltipRect.height - 10;
    
    if (left < 10) left = 10;
    if (left + tooltipRect.width > viewportWidth - 10) {
        left = viewportWidth - tooltipRect.width - 10;
    }
    
    if (top < 10) {
        top = rect.bottom + 10;
    }
    
    tooltip.style.left = left + 'px';
    tooltip.style.top = top + 'px';
}

// Enhanced table interactions with hover effects and focus management
function initializeTableInteractions() {
    const tableRows = document.querySelectorAll('.table tbody tr');
    
    tableRows.forEach(row => {
        row.addEventListener('mouseenter', function() {
            this.classList.add('hovered');
            
            const avatar = this.querySelector('.player-avatar');
            if (avatar) {
                avatar.style.transform = 'scale(1.1)';
            }
        });
        
        row.addEventListener('mouseleave', function() {
            this.classList.remove('hovered');
            
            const avatar = this.querySelector('.player-avatar');
            if (avatar) {
                avatar.style.transform = '';
            }
        });
        
        row.addEventListener('click', function() {
            const playerName = this.querySelector('.player-details h6');
            if (playerName) {
                console.log('Selected player:', playerName.textContent);
            }
        });
        
        row.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });
    });
}

// Animate player statistics with progressive reveal
function initializePlayerStats() {
    const winRateBars = document.querySelectorAll('.win-rate-fill');
    const statusBadges = document.querySelectorAll('.status-badge');
    
    winRateBars.forEach(bar => {
        const width = bar.getAttribute('data-width');
        if (width) {
            bar.style.setProperty('--win-rate-width', width);
        }
    });
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                if (entry.target.classList.contains('win-rate-fill')) {
                    animateWinRateBar(entry.target);
                } else if (entry.target.classList.contains('status-badge')) {
                    animateStatusBadge(entry.target);
                }
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });
    
    winRateBars.forEach(bar => observer.observe(bar));
    statusBadges.forEach(badge => observer.observe(badge));
}

// Animate win rate bar with smooth fill effect
function animateWinRateBar(bar) {
    const finalWidth = bar.style.width;
    bar.style.width = '0%';
    bar.style.transition = 'width 1.5s cubic-bezier(0.4, 0, 0.2, 1)';
    
    setTimeout(() => {
        bar.style.width = finalWidth;
    }, 100);
}

// Animate status badge with pulse effect
function animateStatusBadge(badge) {
    badge.style.animation = 'statusPulse 0.8s ease-out';
    
    setTimeout(() => {
        badge.style.animation = '';
    }, 800);
}

function showNoResults(table) {
    hideNoResults(table);
    
    const noResultsRow = document.createElement('tr');
    noResultsRow.className = 'no-results-row';
    noResultsRow.innerHTML = `
        <td colspan="100%" class="no-results">
            <div class="no-results-content">
                <div class="no-results-icon">🔍</div>
                <h4>No players found</h4>
                <p>Try adjusting your search criteria or check back later.</p>
            </div>
        </td>
    `;
    
    table.appendChild(noResultsRow);
    
    setTimeout(() => {
        noResultsRow.classList.add('visible');
    }, 50);
}

function hideNoResults(table) {
    const noResultsRow = table.querySelector('.no-results-row');
    if (noResultsRow) {
        noResultsRow.remove();
    }
}

function updateResultsCount(visible, total) {
    let countElement = document.querySelector('.results-count');
    
    if (!countElement) {
        countElement = document.createElement('div');
        countElement.className = 'results-count';
        const tableHeader = document.querySelector('.table-header');
        if (tableHeader) {
            tableHeader.appendChild(countElement);
        }
    }
    
    if (visible !== total) {
        countElement.textContent = `Showing ${visible} of ${total} players`;
        countElement.style.display = 'block';
    } else {
        countElement.style.display = 'none';
    }
}

const additionalStyles = `
    .search-input.focused {
        transform: translateY(-2px);
        box-shadow: 0 8px 25px rgba(66, 153, 225, 0.2);
    }
    
    .search-result {
        animation: fadeInResult 0.4s ease-out;
    }
    
    @keyframes fadeInResult {
        from {
            opacity: 0;
            transform: translateY(10px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
    
    .table th.sorting {
        background: rgba(66, 153, 225, 0.1);
        transform: scale(0.98);
    }
    
    .table tbody tr.hovered {
        background: rgba(66, 153, 225, 0.05);
        transform: translateX(8px);
    }
    
    .no-results-content {
        text-align: center;
        padding: 3rem;
        color: #718096;
    }
    
    .no-results-icon {
        font-size: 3rem;
        margin-bottom: 1rem;
    }
    
    .no-results-row {
        opacity: 0;
        transition: opacity 0.3s ease;
    }
    
    .no-results-row.visible {
        opacity: 1;
    }
    
    .results-count {
        font-size: 0.9rem;
        color: #718096;
        font-weight: 500;
        margin-left: auto;
    }
    
    @keyframes statusPulse {
        0% {
            transform: scale(1);
        }
        50% {
            transform: scale(1.1);
        }
        100% {
            transform: scale(1);
        }
    }
`;

const style = document.createElement('style');
style.textContent = additionalStyles;
document.head.appendChild(style); 