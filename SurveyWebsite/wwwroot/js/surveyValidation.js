// surveyValidation.js

// Hàm kiểm tra tiêu đề khảo sát
function validateTitle() {
    const titleInput = document.querySelector('input[name="Title"]');
    const errorSpan = document.querySelector('span[data-valmsg-for="Title"]');
    if (!titleInput.value.trim()) {
        errorSpan.textContent = "Tiêu đề khảo sát là bắt buộc.";
        titleInput.classList.add('is-invalid');
        return false;
    } else {
        errorSpan.textContent = "";
        titleInput.classList.remove('is-invalid');
        return true;
    }
}

// Hàm kiểm tra người được phép truy cập
function validateAllowedUsers() {
    const isPublic = document.getElementById('isPublicCheckbox').checked;
    const allowedUsersSelect = document.getElementById('allowedUsersSelect');
    const errorSpan = document.getElementById('allowedUsersError');
    if (!isPublic && allowedUsersSelect.selectedOptions.length === 0) {
        errorSpan.textContent = "Vui lòng chọn ít nhất một người dùng được phép truy cập.";
        allowedUsersSelect.classList.add('is-invalid');
        return false;
    } else {
        errorSpan.textContent = "";
        allowedUsersSelect.classList.remove('is-invalid');
        return true;
    }
}

// Hàm kiểm tra nội dung câu hỏi
function validateQuestionText(questionItem) {
    const textInput = questionItem.querySelector('.question-text');
    const errorSpan = questionItem.querySelector('.question-text-error');
    if (!textInput.value.trim()) {
        errorSpan.textContent = "Nội dung câu hỏi là bắt buộc.";
        textInput.classList.add('is-invalid');
        return false;
    } else {
        errorSpan.textContent = "";
        textInput.classList.remove('is-invalid');
        return true;
    }
}

// Hàm kiểm tra lựa chọn cho câu hỏi trắc nghiệm và hộp kiểm
function validateOptions(questionItem) {
    const typeSelect = questionItem.querySelector('.question-type');
    const optionsContainer = questionItem.querySelector('.options-container');
    const errorSpan = questionItem.querySelector('.options-error');
    if (typeSelect.value !== "Text") {
        const optionInputs = optionsContainer.querySelectorAll('.option-text');
        const options = Array.from(optionInputs).map(input => input.value.trim());
        const uniqueOptions = new Set(options.filter(opt => opt !== ""));
        if (uniqueOptions.size < 2) {
            errorSpan.textContent = "Phải có ít nhất 2 lựa chọn khác nhau.";
            optionInputs.forEach(input => input.classList.add('is-invalid'));
            return false;
        } else if (options.length !== uniqueOptions.size) {
            errorSpan.textContent = "Các lựa chọn không được trùng lặp.";
            optionInputs.forEach(input => input.classList.add('is-invalid'));
            return false;
        } else {
            errorSpan.textContent = "";
            optionInputs.forEach(input => input.classList.remove('is-invalid'));
            return true;
        }
    } else {
        errorSpan.textContent = "";
        return true;
    }
}

// Hàm kiểm tra một câu hỏi cụ thể
function validateQuestion(questionItem) {
    const questionValid = validateQuestionText(questionItem);
    const optionsValid = validateOptions(questionItem);
    return questionValid && optionsValid;
}

// Hàm kiểm tra tất cả câu hỏi
function validateAllQuestions() {
    let isValid = true;
    const questionItems = document.querySelectorAll('.question-item');
    questionItems.forEach(questionItem => {
        const questionValid = validateQuestionText(questionItem);
        const optionsValid = validateOptions(questionItem);
        isValid = questionValid && optionsValid && isValid;
    });
    return isValid;
}

// Hàm kiểm tra toàn bộ form
function validateForm() {
    let isValid = true;
    isValid = validateTitle() && isValid;
    isValid = validateAllowedUsers() && isValid;
    isValid = validateAllQuestions() && isValid;

    if (!isValid) {
        alert("Có lỗi trong form. Vui lòng kiểm tra và sửa các trường có lỗi.");
        const firstInvalid = document.querySelector('.is-invalid');
        if (firstInvalid) {
            firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            firstInvalid.focus();
        }
        return false; // Ngăn submit
    }
    return true; // Cho phép submit
}

// Thêm sự kiện cho các trường
document.addEventListener('DOMContentLoaded', function () {
    // Tiêu đề
    const titleInput = document.querySelector('input[name="Title"]');
    titleInput.addEventListener('blur', validateTitle);
    titleInput.addEventListener('focus', () => {
        const errorSpan = document.querySelector('span[data-valmsg-for="Title"]');
        errorSpan.textContent = "";
        titleInput.classList.remove('is-invalid');
    });
    titleInput.addEventListener('input', validateTitle);

    // Người được phép truy cập
    const isPublicCheckbox = document.getElementById('isPublicCheckbox');
    isPublicCheckbox.addEventListener('change', validateAllowedUsers);
    const allowedUsersSelect = document.getElementById('allowedUsersSelect');
    allowedUsersSelect.addEventListener('change', validateAllowedUsers);

    // Câu hỏi và lựa chọn
    document.querySelectorAll('.question-text').forEach(input => {
        input.addEventListener('blur', function () {
            const questionItem = this.closest('.question-item');
            validateQuestionText(questionItem);
        });
        input.addEventListener('focus', function () {
            const questionItem = this.closest('.question-item');
            const errorSpan = questionItem.querySelector('.question-text-error');
            errorSpan.textContent = "";
            this.classList.remove('is-invalid');
        });
        input.addEventListener('input', function () {
            const questionItem = this.closest('.question-item');
            validateQuestionText(questionItem);
        });
    });

    document.querySelectorAll('.option-text').forEach(input => {
        input.addEventListener('input', function () {
            const questionItem = this.closest('.question-item');
            validateOptions(questionItem);
        });
    });

    // Kiểm tra khi submit form
    const surveyForm = document.getElementById('surveyForm');
    if (surveyForm) {
        surveyForm.addEventListener('submit', function (event) {
            console.log("Submit event triggered");
            if (!validateForm()) {
                console.log("Validation failed, preventing submit");
                event.preventDefault();
            } else {
                console.log("Validation passed, allowing submit");
            }
        });
    } else {
        console.error("Form with ID 'surveyForm' not found");
    }
});