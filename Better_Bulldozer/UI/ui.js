if (yyBetterBulldozer == null) var yyBetterBulldozer = {};

if (typeof yyBetterBulldozer.CheckForElementByID !== 'function') {
    yyBetterBulldozer.CheckForElementByID = function (id)
    {
        if (document.getElementById(id) != null) {
            engine.trigger('CheckForElement-'+id , true);
            return;
        }
        engine.trigger('CheckForElement-' + id, false);
    }
}

if (typeof yyBetterBulldozer.setupButton !== 'function') {
    yyBetterBulldozer.setupButton = function(buttonId, selected, toolTipKey) {
        const button = document.getElementById(buttonId);
        if (button == null) {
            engine.trigger('YYBB-log', "JS Error: could not setup button " + buttonId);
            return;
        }
        if (selected) {
            button.classList.add("selected");
        } else {
            button.classList.remove("selected");
        }
        button.onclick = function () {
            let selected = true;
            if (this.classList.contains("selected")) {
                selected = false; // This is intended to toggle and be the opposite of what it is now.
            }
            const thisButton = document.getElementById(this.id);
            if (selected) {
                thisButton.classList.add("selected");
            } else {
                thisButton.classList.remove("selected");
            }
            engine.trigger(this.id, selected);
        }
        yyBetterBulldozer.setTooltip(buttonId, toolTipKey);
    }
}

if (typeof yyBetterBulldozer.setupToolButton !== 'function') {
    yyBetterBulldozer.setupToolButton = function (buttonId, selected, toolTipKey) {
        const button = document.getElementById(buttonId);
        if (button == null) {
            engine.trigger('YYA-log', "JS Error: could not setup button " + buttonId);
            return;
        }
        if (selected) {
            button.classList.add("selected");
        } else {
            button.classList.remove("selected");
        }
        button.onclick = function () {
            engine.trigger(this.id);
        }
        yyBetterBulldozer.setTooltip(buttonId, toolTipKey);
    }
}

// Function to apply translation strings.
if (typeof yyBetterBulldozer.applyLocalization !== 'function') {
    yyBetterBulldozer.applyLocalization = function (target) {
        if (!target) {
            return;
        }

        let targets = target.querySelectorAll('[localeKey]');
        targets.forEach(function (currentValue) {
            currentValue.innerHTML = engine.translate(currentValue.getAttribute("localeKey"));
        });
    }
}

// Function to setup tooltip.
if (typeof yyBetterBulldozer.setTooltip !== 'function') {
    yyBetterBulldozer.setTooltip = function (id, toolTipKey) {
        let target = document.getElementById(id);
        target.onmouseenter = () => yyBetterBulldozer.showTooltip(document.getElementById(id), toolTipKey);
        target.onmouseleave = yyBetterBulldozer.hideTooltip;
    }
}

// Function to show a tooltip, creating if necessary.
if (typeof yyBetterBulldozer.showTooltip !== 'function') {
    yyBetterBulldozer.showTooltip = function (parent, tooltipKey) {

        if (!document.getElementById("yyBetterBulldozerToolip")) {
            yyBetterBulldozer.tooltip = document.createElement("div");
            yyBetterBulldozer.tooltip.id = "yyBetterBulldozerToolip";
            yyBetterBulldozer.tooltip.style.visibility = "hidden";
            yyBetterBulldozer.tooltip.classList.add("balloon_qJY", "balloon_H23", "up_ehW", "center_hug", "anchored-balloon_AYp", "up_el0");
            let boundsDiv = document.createElement("div");
            boundsDiv.classList.add("bounds__AO");
            let containerDiv = document.createElement("div");
            containerDiv.classList.add("container_zgM", "container_jfe");
            let contentDiv = document.createElement("div");
            contentDiv.classList.add("content_A82", "content_JQV");
            let arrowDiv = document.createElement("div");
            arrowDiv.classList.add("arrow_SVb", "arrow_Xfn");
            let broadDiv = document.createElement("div");
            yyBetterBulldozer.tooltipTitle = document.createElement("div");
            yyBetterBulldozer.tooltipTitle.classList.add("title_lCJ");
            let paraDiv = document.createElement("div");
            paraDiv.classList.add("paragraphs_nbD", "description_dNa");
            yyBetterBulldozer.tooltipPara = document.createElement("p");
            yyBetterBulldozer.tooltipPara.setAttribute("cohinline", "cohinline");

            paraDiv.appendChild(yyBetterBulldozer.tooltipPara);
            broadDiv.appendChild(yyBetterBulldozer.tooltipTitle);
            broadDiv.appendChild(paraDiv);
            containerDiv.appendChild(arrowDiv);
            contentDiv.appendChild(broadDiv);
            boundsDiv.appendChild(containerDiv);
            boundsDiv.appendChild(contentDiv);
            yyBetterBulldozer.tooltip.appendChild(boundsDiv);

            // Append tooltip to screen element.
            let screenParent = document.getElementsByClassName("game-main-screen_TRK");
            if (screenParent.length == 0) {
                screenParent = document.getElementsByClassName("editor-main-screen_m89");
            }
            if (screenParent.length > 0) {
                screenParent[0].appendChild(yyBetterBulldozer.tooltip);
            }
        }

        // Set text and position.
        yyBetterBulldozer.tooltipTitle.innerHTML = engine.translate("YY_BETTER_BULLDOZER." + tooltipKey);
        yyBetterBulldozer.tooltipPara.innerHTML = engine.translate("YY_BETTER_BULLDOZER_DESCRIPTION." + tooltipKey);

        // Set visibility tracking to prevent race conditions with popup delay.
        yyBetterBulldozer.tooltipVisibility = "visible";

        // Slightly delay popup by three frames to prevent premature activation and to ensure layout is ready.
        window.requestAnimationFrame(() => {
            window.requestAnimationFrame(() => {
                window.requestAnimationFrame(() => {
                    yyBetterBulldozer.setTooltipPos(parent);
                });

            });
        });
    }
}

// Function to adjust the position of a tooltip and make visible.
if (typeof yyBetterBulldozer.setTooltipPos !== 'function') {
    yyBetterBulldozer.setTooltipPos = function (parent) {
        if (!yyBetterBulldozer.tooltip) {
            return;
        }

        let tooltipRect = yyBetterBulldozer.tooltip.getBoundingClientRect();
        let parentRect = parent.getBoundingClientRect();
        let xPos = parentRect.left + ((parentRect.width - tooltipRect.width) / 2);
        let yPos = parentRect.top - tooltipRect.height;
        yyBetterBulldozer.tooltip.setAttribute("style", "left:" + xPos + "px; top: " + yPos + "px; --posY: " + yPos + "px; --posX:" + xPos + "px");

        yyBetterBulldozer.tooltip.style.visibility = yyBetterBulldozer.tooltipVisibility;
    }
}

// Function to hide the tooltip.
if (typeof yyBetterBulldozer.hideTooltip !== 'function') {
    yyBetterBulldozer.hideTooltip = function () {
        if (yyBetterBulldozer.tooltip) {
            yyBetterBulldozer.tooltipVisibility = "hidden";
            yyBetterBulldozer.tooltip.style.visibility = "hidden";
        }
    }
}