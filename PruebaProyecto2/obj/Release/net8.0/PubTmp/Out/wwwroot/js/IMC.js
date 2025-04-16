function calculateBMI() {
    const weight = Number.parseFloat(document.getElementById("weight").value)
    const height = Number.parseFloat(document.getElementById("height").value) / 100 // convert cm to m
    const resultDiv = document.getElementById("result")
    const calculator = document.getElementById("calculator")

    if (isNaN(weight) || isNaN(height) || weight <= 0 || height <= 0) {
        resultDiv.textContent = "Por favor, ingresa valores válidos."
        calculator.className = "calculator"
        return
    }

    const bmi = weight / (height * height)
    let interpretation = ""
    let colorClass = ""

    if (bmi < 18.5) {
        interpretation = "Bajo peso"
        colorClass = "warning"
    } else if (bmi < 25) {
        interpretation = "Peso normal"
        colorClass = "normal"
    } else if (bmi < 30) {
        interpretation = "Sobrepeso"
        colorClass = "warning"
    } else {
        interpretation = "Obesidad"
        colorClass = "danger"
    }

    resultDiv.textContent = `Tu IMC es ${bmi.toFixed(2)}. ${interpretation}.`
    calculator.className = `calculator ${colorClass}`
}

